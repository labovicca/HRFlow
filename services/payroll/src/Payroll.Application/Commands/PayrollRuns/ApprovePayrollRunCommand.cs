using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollRuns;

public record ApprovePayrollRunCommand(Guid PayrollRunId, string ApprovedBy) : IRequest<PayrollRunDto>;
