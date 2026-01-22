using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollConfigurations;

public record UpdatePayrollConfigurationCommand(
    Guid Id, 
    UpdatePayrollConfigurationRequest Request, 
    string UpdatedBy) : IRequest<PayrollConfigurationDto>;
