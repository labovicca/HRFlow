using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Queries.PayrollConfigurations;

public record GetPayrollConfigurationByIdQuery(Guid Id) : IRequest<PayrollConfigurationDto?>;
