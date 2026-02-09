using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollRuns;

/// <summary>
/// Command to mark an approved payroll run as Paid.
/// Final step in the payroll workflow: Draft → Calculated → Approved → Paid
/// </summary>
public record MarkPayrollAsPaidCommand(Guid PayrollRunId, string PaidBy) : IRequest<PayrollRunDto>;
