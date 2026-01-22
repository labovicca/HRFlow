using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.Percentage)
            .HasPrecision(5, 2);
        
        builder.HasOne(e => e.PayrollRun)
            .WithMany(p => p.Components)
            .HasForeignKey(e => e.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.PayrollRunId);
    }
}
