using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Queries.PayrollConfigurations;

public class GetPayrollConfigurationsQueryHandler 
    : IRequestHandler<GetPayrollConfigurationsQuery, IEnumerable<PayrollConfigurationDto>>
{
    private readonly IPayrollConfigurationRepository _repository;
    private readonly IMapper _mapper;

    public GetPayrollConfigurationsQueryHandler(
        IPayrollConfigurationRepository repository, 
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PayrollConfigurationDto>> Handle(
        GetPayrollConfigurationsQuery query, 
        CancellationToken cancellationToken)
    {
        var configurations = await _repository.GetAllAsync(cancellationToken);
        
        if (query.EmployeeId.HasValue)
        {
            configurations = configurations.Where(c => c.EmployeeId == query.EmployeeId.Value);
        }

        return _mapper.Map<IEnumerable<PayrollConfigurationDto>>(configurations);
    }
}
