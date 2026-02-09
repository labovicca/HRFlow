using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollRuns;

public record CalculatePayrollCommand(Guid PayrollRunId, string UpdatedBy) : IRequest<PayrollCalculationResult>;
