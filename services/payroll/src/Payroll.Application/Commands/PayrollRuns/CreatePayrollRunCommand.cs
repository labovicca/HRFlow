using MediatR;
using Payroll.Application.DTOs;

namespace Payroll.Application.Commands.PayrollRuns;

public record CreatePayrollRunCommand(CreatePayrollRunRequest Request, string CreatedBy) : IRequest<PayrollRunDto>;
