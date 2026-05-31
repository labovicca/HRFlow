using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Queries.PayrollConfigurations;

public record GetPayrollConfigurationsQuery(Guid? EmployeeId = null) : IRequest<IEnumerable<PayrollConfigurationDto>>;
