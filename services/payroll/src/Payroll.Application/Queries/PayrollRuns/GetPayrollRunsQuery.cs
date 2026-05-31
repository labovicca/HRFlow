using MediatR;
using Payroll.Application.DTOs;
using Payroll.Domain.Enums;

namespace Payroll.Application.Queries.PayrollRuns;

public record GetPayrollRunsQuery(
    Guid? EmployeeId = null,
    int? Year = null,
    int? Month = null,
    PayrollStatus? Status = null) : IRequest<IEnumerable<PayrollRunListDto>>;
