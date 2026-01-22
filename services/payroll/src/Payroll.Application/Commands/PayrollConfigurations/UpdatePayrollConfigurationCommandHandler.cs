using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands.PayrollConfigurations;

public class UpdatePayrollConfigurationCommandHandler 
    : IRequestHandler<UpdatePayrollConfigurationCommand, PayrollConfigurationDto>
{
    private readonly IPayrollConfigurationRepository _repository;
    private readonly IMapper _mapper;

    public UpdatePayrollConfigurationCommandHandler(
        IPayrollConfigurationRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PayrollConfigurationDto> Handle(
        UpdatePayrollConfigurationCommand command, 
        CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(command.Id, cancellationToken);
        
        if (configuration == null)
        {
            throw new KeyNotFoundException($"Payroll configuration with ID {command.Id} not found");
        }

        var request = command.Request;
        
        configuration.BaseSalary = request.BaseSalary;
        configuration.TaxRate = request.TaxRate;
        configuration.PensionContributionRate = request.PensionContributionRate;
        configuration.HealthInsuranceRate = request.HealthInsuranceRate;
        configuration.UnemploymentInsuranceRate = request.UnemploymentInsuranceRate;
        configuration.EffectiveFrom = request.EffectiveFrom;
        configuration.EffectiveTo = request.EffectiveTo;
        configuration.UpdatedBy = command.UpdatedBy;

        await _repository.UpdateAsync(configuration, cancellationToken);
        
        return _mapper.Map<PayrollConfigurationDto>(configuration);
    }
}
