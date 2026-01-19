using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.GrossSalary)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.NetSalary)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.TotalDeductions)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.TotalAdditions)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256);
        
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);
        
        // Index for quick lookups by employee and period
        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.Month })
            .IsUnique();
        
        builder.HasIndex(e => e.Status);
    }
}
