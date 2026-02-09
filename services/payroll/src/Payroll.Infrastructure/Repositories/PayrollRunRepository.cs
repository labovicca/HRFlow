using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Infrastructure.Repositories;

public class PayrollRunRepository : IPayrollRunRepository
{
    private readonly PayrollDbContext _context;

    public PayrollRunRepository(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PayrollRun?> GetByIdWithComponentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns
            .Include(p => p.Components)
            .Include(p => p.Payslip)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PayrollRun?> GetByEmployeeAndPeriodAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns
            .Include(p => p.Components)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Year == year && p.Month == month, cancellationToken);
    }

    public async Task<IEnumerable<PayrollRun>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PayrollRun>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PayrollRun>> GetByStatusAsync(PayrollStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PayrollRun>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns
            .Where(p => p.Year == year && p.Month == month)
            .OrderBy(p => p.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PayrollRun> AddAsync(PayrollRun payrollRun, CancellationToken cancellationToken = default)
    {
        payrollRun.Id = Guid.NewGuid();
        payrollRun.CreatedAt = DateTime.UtcNow;
        
        await _context.PayrollRuns.AddAsync(payrollRun, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return payrollRun;
    }

    public async Task UpdateAsync(PayrollRun payrollRun, CancellationToken cancellationToken = default)
    {
        payrollRun.UpdatedAt = DateTime.UtcNow;
        
        _context.PayrollRuns.Update(payrollRun);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payrollRun = await GetByIdAsync(id, cancellationToken);
        if (payrollRun != null)
        {
            _context.PayrollRuns.Remove(payrollRun);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollRuns.AnyAsync(p => p.Id == id, cancellationToken);
    }
}
