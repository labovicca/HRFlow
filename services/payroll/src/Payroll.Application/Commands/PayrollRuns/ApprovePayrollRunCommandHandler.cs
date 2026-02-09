using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class ApprovePayrollRunCommandHandler : IRequestHandler<ApprovePayrollRunCommand, PayrollRunDto>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public ApprovePayrollRunCommandHandler(
        IPayrollRunRepository payrollRunRepository,
        IMapper mapper,
        IEventPublisher eventPublisher)
    {
        _payrollRunRepository = payrollRunRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<PayrollRunDto> Handle(ApprovePayrollRunCommand command, CancellationToken cancellationToken)
    {
        var payrollRun = await _payrollRunRepository.GetByIdWithComponentsAsync(command.PayrollRunId, cancellationToken);
        
        if (payrollRun == null)
        {
            throw new KeyNotFoundException($"Payroll run with ID {command.PayrollRunId} not found");
        }

        if (payrollRun.Status != PayrollStatus.Calculated)
        {
            throw new InvalidOperationException(
                $"Cannot approve payroll run with status {payrollRun.Status}. Only Calculated status can be approved.");
        }

        payrollRun.Status = PayrollStatus.Approved;
        payrollRun.UpdatedBy = command.ApprovedBy;

        await _payrollRunRepository.UpdateAsync(payrollRun, cancellationToken);

        // Publish event to RabbitMQ
        await _eventPublisher.PublishAsync(new PayrollApprovedEvent
        {
            PayrollRunId = payrollRun.Id,
            EmployeeId = payrollRun.EmployeeId,
            Year = payrollRun.Year,
            Month = payrollRun.Month,
            GrossSalary = payrollRun.GrossSalary,
            NetSalary = payrollRun.NetSalary,
            TotalDeductions = payrollRun.TotalDeductions,
            ApprovedBy = command.ApprovedBy,
            ApprovedAt = DateTime.UtcNow
        }, cancellationToken);

        return _mapper.Map<PayrollRunDto>(payrollRun);
    }
}
