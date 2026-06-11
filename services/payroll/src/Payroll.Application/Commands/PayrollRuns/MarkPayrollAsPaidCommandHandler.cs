using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class MarkPayrollAsPaidCommandHandler : IRequestHandler<MarkPayrollAsPaidCommand, PayrollRunDto>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public MarkPayrollAsPaidCommandHandler(
        IPayrollRunRepository payrollRunRepository,
        IMapper mapper,
        IEventPublisher eventPublisher)
    {
        _payrollRunRepository = payrollRunRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<PayrollRunDto> Handle(MarkPayrollAsPaidCommand command, CancellationToken cancellationToken)
    {
        var payrollRun = await _payrollRunRepository.GetByIdWithComponentsAsync(command.PayrollRunId, cancellationToken);

        if (payrollRun == null)
        {
            throw new KeyNotFoundException($"Payroll run with ID {command.PayrollRunId} not found.");
        }

        if (payrollRun.Status != PayrollStatus.Approved)
        {
            throw new InvalidOperationException(
                $"Cannot mark payroll run as Paid. Current status is '{payrollRun.Status}', but only 'Approved' payroll runs can be marked as Paid.");
        }

        payrollRun.Status = PayrollStatus.Paid;
        payrollRun.UpdatedAt = DateTime.UtcNow;
        payrollRun.UpdatedBy = command.PaidBy;

        await _payrollRunRepository.UpdateAsync(payrollRun, cancellationToken);

        // Publish event - useful for notification service to inform employees
        await _eventPublisher.PublishAsync(new PayrollPaidEvent
        {
            PayrollRunId = payrollRun.Id,
            EmployeeId = payrollRun.EmployeeId,
            Year = payrollRun.Year,
            Month = payrollRun.Month,
            NetSalary = payrollRun.NetSalary,
            PaidBy = command.PaidBy,
            PaidAt = DateTime.UtcNow
        }, cancellationToken);

        return _mapper.Map<PayrollRunDto>(payrollRun);
    }
}
