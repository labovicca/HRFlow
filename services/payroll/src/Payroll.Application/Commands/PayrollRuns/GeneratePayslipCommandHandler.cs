using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class GeneratePayslipCommandHandler : IRequestHandler<GeneratePayslipCommand, PayslipDto>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IPayrollConfigurationRepository _configurationRepository;
    private readonly IPayslipGenerator _payslipGenerator;
    private readonly IEventPublisher _eventPublisher;

    public GeneratePayslipCommandHandler(
        IPayrollRunRepository payrollRunRepository,
        IPayrollConfigurationRepository configurationRepository,
        IPayslipGenerator payslipGenerator,
        IEventPublisher eventPublisher)
    {
        _payrollRunRepository = payrollRunRepository;
        _configurationRepository = configurationRepository;
        _payslipGenerator = payslipGenerator;
        _eventPublisher = eventPublisher;
    }

    public async Task<PayslipDto> Handle(GeneratePayslipCommand command, CancellationToken cancellationToken)
    {
        var payrollRun = await _payrollRunRepository.GetByIdWithComponentsAsync(command.PayrollRunId, cancellationToken);

        if (payrollRun == null)
        {
            throw new KeyNotFoundException($"Payroll run with ID {command.PayrollRunId} not found");
        }

        if (payrollRun.Status != PayrollStatus.Calculated && payrollRun.Status != PayrollStatus.Approved)
        {
            throw new InvalidOperationException(
                $"Cannot generate payslip for payroll run with status {payrollRun.Status}. Only Calculated or Approved status is allowed.");
        }

        // Get configuration
        var periodDate = new DateTime(payrollRun.Year, payrollRun.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var configuration = await _configurationRepository.GetEffectiveConfigurationAsync(
            payrollRun.EmployeeId, periodDate, cancellationToken);

        if (configuration == null)
        {
            throw new InvalidOperationException(
                $"No payroll configuration found for employee {payrollRun.EmployeeId}");
        }

        // Generate PDF
        var filePath = await _payslipGenerator.GenerateAsync(payrollRun, configuration, cancellationToken);

        // Create or update payslip entity
        var payslip = payrollRun.Payslip ?? new Payslip();
        payslip.PayrollRunId = payrollRun.Id;
        payslip.FilePath = filePath;
        payslip.GeneratedAt = DateTime.UtcNow;
        payslip.GeneratedBy = command.GeneratedBy;

        if (payrollRun.Payslip == null)
        {
            payslip.Id = Guid.NewGuid();
            payrollRun.Payslip = payslip;
        }

        await _payrollRunRepository.UpdateAsync(payrollRun, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new PayslipGeneratedEvent
        {
            PayslipId = payslip.Id,
            PayrollRunId = payrollRun.Id,
            EmployeeId = payrollRun.EmployeeId,
            Year = payrollRun.Year,
            Month = payrollRun.Month,
            FilePath = filePath,
            GeneratedAt = payslip.GeneratedAt
        }, cancellationToken);

        return new PayslipDto
        {
            Id = payslip.Id,
            PayrollRunId = payrollRun.Id,
            FilePath = filePath,
            GeneratedAt = payslip.GeneratedAt,
            GeneratedBy = payslip.GeneratedBy
        };
    }
}
