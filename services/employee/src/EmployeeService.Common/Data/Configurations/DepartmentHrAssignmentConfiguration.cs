using EmployeeService.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeService.Common.Data.Configurations;

public class DepartmentHrAssignmentConfiguration : IEntityTypeConfiguration<DepartmentHrAssignment>
{
    public void Configure(EntityTypeBuilder<DepartmentHrAssignment> builder)
    {
        builder.ToTable("DepartmentHrAssignment");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn();

        builder.Property(a => a.Department).HasMaxLength(150).IsRequired();
        builder.HasIndex(a => a.Department).IsUnique();

        builder.Property(a => a.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(a => a.HrEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.HrEmployeeId);
    }
}
