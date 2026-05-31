using MediatR;

namespace Payroll.Application.Commands.PayrollConfigurations;

public record DeletePayrollConfigurationCommand(Guid Id) : IRequest<bool>;
