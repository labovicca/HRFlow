using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class BulkCalculatePayrollCommandHandler : IRequestHandler<BulkCalculatePayrollCommand, BulkOperationResult>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IPayrollConfigurationRepository _configurationRepository;
    private readonly IPayrollCalculator _payrollCalculator;
    private readonly IEventPublisher _eventPublisher;

    public BulkCalculatePayrollCommandHandler(
        IPayrollRunRepository payrollRunRepository,
        IPayrollConfigurationRepository configurationRepository,
        IPayrollCalculator payrollCalculator,
        IEventPublisher eventPublisher)
    {
        _payrollRunRepository = payrollRunRepository;
        _configurationRepository = configurationRepository;
        _payrollCalculator = payrollCalculator;
        _eventPublisher = eventPublisher;
    }

    public async Task<BulkOperationResult> Handle(BulkCalculatePayrollCommand command, CancellationToken cancellationToken)
    {
        var result = new BulkOperationResult();

        // Get all payroll runs for the period
        var payrollRuns = (await _payrollRunRepository.GetByPeriodAsync(command.Year, command.Month, cancellationToken))
            .Where(pr => pr.Status == PayrollStatus.Draft)
            .ToList();

        result.TotalProcessed = payrollRuns.Count;

        foreach (var payrollRun in payrollRuns)
        {
            try
            {
                // Load components
                var fullRun = await _payrollRunRepository.GetByIdWithComponentsAsync(payrollRun.Id, cancellationToken);
                if (fullRun == null) continue;

                var periodDate = new DateTime(command.Year, command.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var configuration = await _configurationRepository.GetEffectiveConfigurationAsync(
                    fullRun.EmployeeId, periodDate, cancellationToken);

                if (configuration == null)
                {
                    result.Failed++;
                    result.Errors.Add(new BulkOperationError
                    {
                        PayrollRunId = fullRun.Id,
                        EmployeeId = fullRun.EmployeeId,
                        ErrorMessage = $"No configuration found for employee {fullRun.EmployeeId}"
                    });
                    continue;
                }

                var calcResult = await _payrollCalculator.CalculateAsync(fullRun, configuration, cancellationToken);

                fullRun.GrossSalary = calcResult.GrossSalary;
                fullRun.NetSalary = calcResult.NetSalary;
                fullRun.TotalDeductions = calcResult.TotalDeductions;
                fullRun.TotalAdditions = calcResult.TotalAdditions;
                fullRun.Status = PayrollStatus.Calculated;
                fullRun.UpdatedBy = command.UpdatedBy;

                await _payrollRunRepository.UpdateAsync(fullRun, cancellationToken);

                await _eventPublisher.PublishAsync(new PayrollCalculatedEvent
                {
                    PayrollRunId = fullRun.Id,
                    EmployeeId = fullRun.EmployeeId,
                    Year = command.Year,
                    Month = command.Month,
                    GrossSalary = calcResult.GrossSalary,
                    NetSalary = calcResult.NetSalary,
                    TotalDeductions = calcResult.TotalDeductions,
                    CalculatedAt = DateTime.UtcNow
                }, cancellationToken);

                result.Successful++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add(new BulkOperationError
                {
                    PayrollRunId = payrollRun.Id,
                    EmployeeId = payrollRun.EmployeeId,
                    ErrorMessage = ex.Message
                });
            }
        }

        return result;
    }
}
