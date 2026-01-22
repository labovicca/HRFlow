using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollConfigurations;

public record CreatePayrollConfigurationCommand(CreatePayrollConfigurationRequest Request, string CreatedBy) 
    : IRequest<PayrollConfigurationDto>;
