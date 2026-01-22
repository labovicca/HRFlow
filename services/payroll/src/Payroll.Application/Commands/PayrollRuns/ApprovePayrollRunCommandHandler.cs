using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class ApprovePayrollRunCommandHandler : IRequestHandler<ApprovePayrollRunCommand, PayrollRunDto>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IMapper _mapper;

    public ApprovePayrollRunCommandHandler(
        IPayrollRunRepository payrollRunRepository,
        IMapper mapper)
    {
        _payrollRunRepository = payrollRunRepository;
        _mapper = mapper;
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

        return _mapper.Map<PayrollRunDto>(payrollRun);
    }
}
