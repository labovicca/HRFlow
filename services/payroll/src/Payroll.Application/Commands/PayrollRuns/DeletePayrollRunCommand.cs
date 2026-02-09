using MediatR;

namespace Payroll.Application.Commands.PayrollRuns;

public record DeletePayrollRunCommand(Guid PayrollRunId) : IRequest<bool>;
