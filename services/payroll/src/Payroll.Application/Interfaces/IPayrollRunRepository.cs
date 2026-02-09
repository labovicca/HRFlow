using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Application.Interfaces;

public interface IPayrollRunRepository
{
    Task<PayrollRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PayrollRun?> GetByIdWithComponentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PayrollRun?> GetByEmployeeAndPeriodAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayrollRun>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PayrollRun>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayrollRun>> GetByStatusAsync(PayrollStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayrollRun>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<PayrollRun> AddAsync(PayrollRun payrollRun, CancellationToken cancellationToken = default);
    Task UpdateAsync(PayrollRun payrollRun, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
