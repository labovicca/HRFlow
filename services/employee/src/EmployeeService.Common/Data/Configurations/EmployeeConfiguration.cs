using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeService.Common.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employee");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn();

        builder.Property(e => e.EmployeeNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(e => e.EmployeeNumber).IsUnique();

        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.WorkEmail).HasMaxLength(255).IsRequired();
        builder.HasIndex(e => e.WorkEmail).IsUnique();

        builder.Property(e => e.PersonalEmail).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.JMBG).HasMaxLength(13).IsRequired();
        builder.HasIndex(e => e.JMBG).IsUnique();

        builder.Property(e => e.Department).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Position).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Role).HasDefaultValue(EmployeeRole.Employee).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("system").IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.DeletedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Department);
        builder.HasIndex(e => e.ManagerId);
        builder.HasIndex(e => e.Role);
        builder.HasIndex(e => e.EmploymentStatus);
        builder.HasIndex(e => e.IsDeleted);
    }
}
