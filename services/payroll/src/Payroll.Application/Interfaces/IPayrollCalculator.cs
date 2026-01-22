using Payroll.Application.DTOs;
using Payroll.Domain.Entities;

namespace Payroll.Application.Interfaces;

public interface IPayrollCalculator
{
    Task<PayrollCalculationResult> CalculateAsync(
        PayrollRun payrollRun, 
        PayrollConfiguration configuration,
        CancellationToken cancellationToken = default);
}
