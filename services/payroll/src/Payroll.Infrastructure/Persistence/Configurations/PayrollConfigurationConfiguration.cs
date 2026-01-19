using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayrollConfigurationConfiguration : IEntityTypeConfiguration<PayrollConfiguration>
{
    public void Configure(EntityTypeBuilder<PayrollConfiguration> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.BaseSalary)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.TaxRate)
            .HasPrecision(5, 4);
        
        builder.Property(e => e.PensionContributionRate)
            .HasPrecision(5, 4);
        
        builder.Property(e => e.HealthInsuranceRate)
            .HasPrecision(5, 4);
        
        builder.Property(e => e.UnemploymentInsuranceRate)
            .HasPrecision(5, 4);
        
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256);
        
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);
        
        builder.HasIndex(e => e.EmployeeId);
        builder.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo });
    }
}
