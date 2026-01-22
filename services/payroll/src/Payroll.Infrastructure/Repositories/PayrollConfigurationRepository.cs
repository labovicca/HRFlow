using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Infrastructure.Repositories;

public class PayrollConfigurationRepository : IPayrollConfigurationRepository
{
    private readonly PayrollDbContext _context;

    public PayrollConfigurationRepository(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollConfigurations
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<PayrollConfiguration?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollConfigurations
            .Where(c => c.EmployeeId == employeeId)
            .Where(c => c.EffectiveTo == null || c.EffectiveTo > DateTime.UtcNow)
            .OrderByDescending(c => c.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PayrollConfiguration?> GetGlobalConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PayrollConfigurations
            .Where(c => c.EmployeeId == null)
            .Where(c => c.EffectiveTo == null || c.EffectiveTo > DateTime.UtcNow)
            .OrderByDescending(c => c.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PayrollConfiguration?> GetEffectiveConfigurationAsync(Guid? employeeId, DateTime date, CancellationToken cancellationToken = default)
    {
        // First try to get employee-specific configuration
        if (employeeId.HasValue)
        {
            var employeeConfig = await _context.PayrollConfigurations
                .Where(c => c.EmployeeId == employeeId)
                .Where(c => c.EffectiveFrom <= date)
                .Where(c => c.EffectiveTo == null || c.EffectiveTo > date)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefaultAsync(cancellationToken);

            if (employeeConfig != null)
                return employeeConfig;
        }

        // Fall back to global configuration
        return await _context.PayrollConfigurations
            .Where(c => c.EmployeeId == null)
            .Where(c => c.EffectiveFrom <= date)
            .Where(c => c.EffectiveTo == null || c.EffectiveTo > date)
            .OrderByDescending(c => c.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<PayrollConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PayrollConfigurations
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PayrollConfiguration> AddAsync(PayrollConfiguration configuration, CancellationToken cancellationToken = default)
    {
        configuration.Id = Guid.NewGuid();
        configuration.CreatedAt = DateTime.UtcNow;
        
        await _context.PayrollConfigurations.AddAsync(configuration, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return configuration;
    }

    public async Task UpdateAsync(PayrollConfiguration configuration, CancellationToken cancellationToken = default)
    {
        configuration.UpdatedAt = DateTime.UtcNow;
        
        _context.PayrollConfigurations.Update(configuration);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var configuration = await GetByIdAsync(id, cancellationToken);
        if (configuration != null)
        {
            _context.PayrollConfigurations.Remove(configuration);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollConfigurations.AnyAsync(c => c.Id == id, cancellationToken);
    }
}
