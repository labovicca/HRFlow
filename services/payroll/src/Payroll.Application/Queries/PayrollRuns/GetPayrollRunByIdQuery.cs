using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Queries.PayrollRuns;

public record GetPayrollRunByIdQuery(Guid Id) : IRequest<PayrollRunDto?>;
