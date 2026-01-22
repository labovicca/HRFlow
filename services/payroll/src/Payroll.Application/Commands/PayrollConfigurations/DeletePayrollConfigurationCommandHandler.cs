using MediatR;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands.PayrollConfigurations;

public class DeletePayrollConfigurationCommandHandler : IRequestHandler<DeletePayrollConfigurationCommand, bool>
{
    private readonly IPayrollConfigurationRepository _repository;

    public DeletePayrollConfigurationCommandHandler(IPayrollConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeletePayrollConfigurationCommand command, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(command.Id, cancellationToken);
        
        if (configuration == null)
        {
            throw new KeyNotFoundException($"Payroll configuration with ID {command.Id} not found");
        }

        await _repository.DeleteAsync(command.Id, cancellationToken);
        
        return true;
    }
}
