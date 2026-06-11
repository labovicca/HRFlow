using EmployeeService.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeService.Common.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Document");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).UseIdentityColumn();

        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(d => d.OriginalFileName).HasMaxLength(255).HasDefaultValue(string.Empty).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(150).HasDefaultValue("application/octet-stream").IsRequired();
        builder.Property(d => d.FileSize).HasDefaultValue(0L);
        builder.Property(d => d.UploadedAt).HasDefaultValueSql("NOW()");
        builder.Property(d => d.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(d => d.CreatedBy).HasMaxLength(100).HasDefaultValue("system").IsRequired();
        builder.Property(d => d.UpdatedBy).HasMaxLength(100);
        builder.Property(d => d.DeletedBy).HasMaxLength(100);
        builder.Property(d => d.IsDeleted).HasDefaultValue(false);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.EmployeeId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.ExpirationDate);
        builder.HasIndex(d => d.IsDeleted);
    }
}
