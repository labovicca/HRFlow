using MediatR;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class DeletePayrollRunCommandHandler : IRequestHandler<DeletePayrollRunCommand, bool>
{
    private readonly IPayrollRunRepository _payrollRunRepository;

    public DeletePayrollRunCommandHandler(IPayrollRunRepository payrollRunRepository)
    {
        _payrollRunRepository = payrollRunRepository;
    }

    public async Task<bool> Handle(DeletePayrollRunCommand command, CancellationToken cancellationToken)
    {
        var payrollRun = await _payrollRunRepository.GetByIdAsync(command.PayrollRunId, cancellationToken);
        
        if (payrollRun == null)
        {
            throw new KeyNotFoundException($"Payroll run with ID {command.PayrollRunId} not found");
        }

        if (payrollRun.Status != PayrollStatus.Draft && payrollRun.Status != PayrollStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot delete payroll run with status {payrollRun.Status}. Only Draft or Cancelled status can be deleted.");
        }

        await _payrollRunRepository.DeleteAsync(command.PayrollRunId, cancellationToken);
        
        return true;
    }
}
