using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Queries.PayrollRuns;

public class GetPayrollRunByIdQueryHandler : IRequestHandler<GetPayrollRunByIdQuery, PayrollRunDto?>
{
    private readonly IPayrollRunRepository _repository;
    private readonly IMapper _mapper;

    public GetPayrollRunByIdQueryHandler(IPayrollRunRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PayrollRunDto?> Handle(GetPayrollRunByIdQuery query, CancellationToken cancellationToken)
    {
        var payrollRun = await _repository.GetByIdWithComponentsAsync(query.Id, cancellationToken);
        
        return payrollRun == null ? null : _mapper.Map<PayrollRunDto>(payrollRun);
    }
}
