using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;

namespace Payroll.Application.Commands.PayrollConfigurations;

public class CreatePayrollConfigurationCommandHandler 
    : IRequestHandler<CreatePayrollConfigurationCommand, PayrollConfigurationDto>
{
    private readonly IPayrollConfigurationRepository _repository;
    private readonly IMapper _mapper;

    public CreatePayrollConfigurationCommandHandler(
        IPayrollConfigurationRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PayrollConfigurationDto> Handle(
        CreatePayrollConfigurationCommand command, 
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var configuration = new PayrollConfiguration
        {
            EmployeeId = request.EmployeeId,
            BaseSalary = request.BaseSalary,
            TaxRate = request.TaxRate,
            PensionContributionRate = request.PensionContributionRate,
            HealthInsuranceRate = request.HealthInsuranceRate,
            UnemploymentInsuranceRate = request.UnemploymentInsuranceRate,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            CreatedBy = command.CreatedBy
        };

        var created = await _repository.AddAsync(configuration, cancellationToken);
        
        return _mapper.Map<PayrollConfigurationDto>(created);
    }
}
