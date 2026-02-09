using Payroll.Domain.Entities;

namespace Payroll.Application.Interfaces;

public interface IPayrollConfigurationRepository
{
    Task<PayrollConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PayrollConfiguration?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<PayrollConfiguration?> GetGlobalConfigurationAsync(CancellationToken cancellationToken = default);
    Task<PayrollConfiguration?> GetEffectiveConfigurationAsync(Guid? employeeId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayrollConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PayrollConfiguration> AddAsync(PayrollConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(PayrollConfiguration configuration, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
