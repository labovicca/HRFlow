using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Persistence;

public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<PayrollRun> PayrollRuns { get; set; }
    public DbSet<SalaryComponent> SalaryComponents { get; set; }
    public DbSet<PayrollConfiguration> PayrollConfigurations { get; set; }
    public DbSet<Payslip> Payslips { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PayrollDbContext).Assembly);
    }
}
