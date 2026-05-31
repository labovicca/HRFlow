using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Persistence.Configurations;

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FilePath)
            .HasMaxLength(1024)
            .IsRequired();
        
        builder.Property(e => e.FileName)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(e => e.GeneratedBy)
            .HasMaxLength(256);
        
        builder.HasOne(e => e.PayrollRun)
            .WithOne(p => p.Payslip)
            .HasForeignKey<Payslip>(e => e.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.PayrollRunId)
            .IsUnique();
    }
}
