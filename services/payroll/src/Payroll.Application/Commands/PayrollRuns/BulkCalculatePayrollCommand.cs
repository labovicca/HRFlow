using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollRuns;

/// <summary>
/// Command for bulk calculating all Draft payroll runs for a given period.
/// Used by HR when they want to process an entire month at once.
/// </summary>
public record BulkCalculatePayrollCommand(int Year, int Month, string UpdatedBy) : IRequest<BulkOperationResult>;
