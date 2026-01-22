using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Queries.PayrollConfigurations;

public class GetPayrollConfigurationByIdQueryHandler 
    : IRequestHandler<GetPayrollConfigurationByIdQuery, PayrollConfigurationDto?>
{
    private readonly IPayrollConfigurationRepository _repository;
    private readonly IMapper _mapper;

    public GetPayrollConfigurationByIdQueryHandler(
        IPayrollConfigurationRepository repository, 
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PayrollConfigurationDto?> Handle(
        GetPayrollConfigurationByIdQuery query, 
        CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(query.Id, cancellationToken);
        
        return configuration == null ? null : _mapper.Map<PayrollConfigurationDto>(configuration);
    }
}
