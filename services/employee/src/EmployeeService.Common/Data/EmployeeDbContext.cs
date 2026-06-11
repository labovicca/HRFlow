using EmployeeService.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Common.Data;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DepartmentHrAssignment> DepartmentHrAssignments => Set<DepartmentHrAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeDbContext).Assembly);
    }
}
