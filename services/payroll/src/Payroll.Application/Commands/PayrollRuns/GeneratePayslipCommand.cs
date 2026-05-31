using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollRuns;

public record GeneratePayslipCommand(Guid PayrollRunId, string GeneratedBy) : IRequest<PayslipDto>;
