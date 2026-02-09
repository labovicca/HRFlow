using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Queries.PayrollRuns;

/// <summary>
/// Query to get a monthly payroll summary report with aggregated data.
/// </summary>
public record GetPayrollSummaryQuery(int Year, int Month) : IRequest<PayrollSummaryDto>;
