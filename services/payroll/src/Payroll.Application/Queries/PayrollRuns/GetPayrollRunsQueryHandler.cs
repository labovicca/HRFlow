using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;

namespace Payroll.Application.Queries.PayrollRuns;

public class GetPayrollRunsQueryHandler : IRequestHandler<GetPayrollRunsQuery, IEnumerable<PayrollRunListDto>>
{
    private readonly IPayrollRunRepository _repository;
    private readonly IMapper _mapper;

    public GetPayrollRunsQueryHandler(IPayrollRunRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PayrollRunListDto>> Handle(GetPayrollRunsQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<PayrollRun> payrollRuns;

        // Apply filters based on query parameters
        if (query.EmployeeId.HasValue)
        {
            payrollRuns = await _repository.GetByEmployeeIdAsync(query.EmployeeId.Value, cancellationToken);
        }
        else if (query.Year.HasValue && query.Month.HasValue)
        {
            payrollRuns = await _repository.GetByPeriodAsync(query.Year.Value, query.Month.Value, cancellationToken);
        }
        else if (query.Status.HasValue)
        {
            payrollRuns = await _repository.GetByStatusAsync(query.Status.Value, cancellationToken);
        }
        else
        {
            payrollRuns = await _repository.GetAllAsync(cancellationToken);
        }

        // Additional filtering in memory if needed
        if (query.Year.HasValue && query.EmployeeId.HasValue)
        {
            payrollRuns = payrollRuns.Where(p => p.Year == query.Year.Value);
        }

        if (query.Month.HasValue && query.EmployeeId.HasValue)
        {
            payrollRuns = payrollRuns.Where(p => p.Month == query.Month.Value);
        }

        return _mapper.Map<IEnumerable<PayrollRunListDto>>(payrollRuns);
    }
}
