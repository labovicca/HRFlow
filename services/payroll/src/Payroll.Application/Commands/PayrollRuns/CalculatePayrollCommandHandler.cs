using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class CalculatePayrollCommandHandler : IRequestHandler<CalculatePayrollCommand, PayrollCalculationResult>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IPayrollConfigurationRepository _configurationRepository;
    private readonly IPayrollCalculator _payrollCalculator;
    private readonly IEventPublisher _eventPublisher;

    public CalculatePayrollCommandHandler(
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

    public async Task<PayrollCalculationResult> Handle(CalculatePayrollCommand command, CancellationToken cancellationToken)
    {
        var payrollRun = await _payrollRunRepository.GetByIdWithComponentsAsync(command.PayrollRunId, cancellationToken);
        
        if (payrollRun == null)
        {
            throw new KeyNotFoundException($"Payroll run with ID {command.PayrollRunId} not found");
        }

        if (payrollRun.Status != PayrollStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot calculate payroll run with status {payrollRun.Status}. Only Draft status is allowed.");
        }

        // Get configuration for calculation
        var periodDate = new DateTime(payrollRun.Year, payrollRun.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var configuration = await _configurationRepository.GetEffectiveConfigurationAsync(
            payrollRun.EmployeeId, periodDate, cancellationToken);

        if (configuration == null)
        {
            throw new InvalidOperationException(
                $"No payroll configuration found for employee {payrollRun.EmployeeId} for period {payrollRun.Year}-{payrollRun.Month:D2}");
        }

        // Calculate payroll
        var result = await _payrollCalculator.CalculateAsync(payrollRun, configuration, cancellationToken);

        // Update payroll run with calculated values
        payrollRun.GrossSalary = result.GrossSalary;
        payrollRun.NetSalary = result.NetSalary;
        payrollRun.TotalDeductions = result.TotalDeductions;
        payrollRun.TotalAdditions = result.TotalAdditions;
        payrollRun.Status = PayrollStatus.Calculated;
        payrollRun.UpdatedBy = command.UpdatedBy;

        await _payrollRunRepository.UpdateAsync(payrollRun, cancellationToken);

        // Publish event to RabbitMQ
        await _eventPublisher.PublishAsync(new PayrollCalculatedEvent
        {
            PayrollRunId = payrollRun.Id,
            EmployeeId = payrollRun.EmployeeId,
            Year = payrollRun.Year,
            Month = payrollRun.Month,
            GrossSalary = result.GrossSalary,
            NetSalary = result.NetSalary,
            TotalDeductions = result.TotalDeductions,
            CalculatedAt = DateTime.UtcNow
        }, cancellationToken);

        return result;
    }
}
