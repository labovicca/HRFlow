using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with sample data for development and demo purposes.
/// </summary>
public static class PayrollDbSeeder
{
    public static async Task SeedAsync(PayrollDbContext context, ILogger logger)
    {
        // Only seed if database is empty
        if (await context.PayrollConfigurations.AnyAsync())
        {
            logger.LogInformation("Database already seeded. Skipping...");
            return;
        }

        logger.LogInformation("Seeding database with sample data...");

        // Sample employee IDs (these would come from the Employee Service in production)
        var employee1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var employee2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var employee3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // 1. Seed Global Payroll Configuration (default for all employees)
        var globalConfig = new PayrollConfiguration
        {
            Id = Guid.NewGuid(),
            EmployeeId = null, // Global
            BaseSalary = 100000m,
            TaxRate = 0.10m,
            PensionContributionRate = 0.14m,
            HealthInsuranceRate = 0.0515m,
            UnemploymentInsuranceRate = 0.0075m,
            EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        // 2. Seed Employee-specific configurations
        var employee1Config = new PayrollConfiguration
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee1Id,
            BaseSalary = 120000m, // Senior developer - higher salary
            TaxRate = 0.10m,
            PensionContributionRate = 0.14m,
            HealthInsuranceRate = 0.0515m,
            UnemploymentInsuranceRate = 0.0075m,
            EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var employee2Config = new PayrollConfiguration
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee2Id,
            BaseSalary = 85000m, // Junior developer
            TaxRate = 0.10m,
            PensionContributionRate = 0.14m,
            HealthInsuranceRate = 0.0515m,
            UnemploymentInsuranceRate = 0.0075m,
            EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var employee3Config = new PayrollConfiguration
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee3Id,
            BaseSalary = 95000m, // Mid developer
            TaxRate = 0.10m,
            PensionContributionRate = 0.14m,
            HealthInsuranceRate = 0.0515m,
            UnemploymentInsuranceRate = 0.0075m,
            EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        context.PayrollConfigurations.AddRange(globalConfig, employee1Config, employee2Config, employee3Config);

        // 3. Seed sample PayrollRuns for January 2026 (Draft status - ready for processing)
        var run1 = new PayrollRun
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee1Id,
            Year = 2026,
            Month = 1,
            Status = PayrollStatus.Draft,
            GrossSalary = 0,
            NetSalary = 0,
            TotalDeductions = 0,
            TotalAdditions = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var run2 = new PayrollRun
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee2Id,
            Year = 2026,
            Month = 1,
            Status = PayrollStatus.Draft,
            GrossSalary = 0,
            NetSalary = 0,
            TotalDeductions = 0,
            TotalAdditions = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var run3 = new PayrollRun
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee3Id,
            Year = 2026,
            Month = 1,
            Status = PayrollStatus.Draft,
            GrossSalary = 0,
            NetSalary = 0,
            TotalDeductions = 0,
            TotalAdditions = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed",
            Components = new List<SalaryComponent>
            {
                new SalaryComponent
                {
                    Id = Guid.NewGuid(),
                    ComponentType = ComponentType.Addition,
                    Name = "Performance Bonus",
                    Amount = 10000m,
                    IsTaxable = true,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        context.PayrollRuns.AddRange(run1, run2, run3);

        // 4. Seed a fully processed payroll (December 2025) to demonstrate completed workflow
        var completedRun = new PayrollRun
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee1Id,
            Year = 2025,
            Month = 12,
            Status = PayrollStatus.Paid,
            GrossSalary = 120000m,
            NetSalary = 96060m,
            TotalDeductions = 23940m,
            TotalAdditions = 0m,
            CreatedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 12, 28, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "seed",
            UpdatedBy = "seed",
            Components = new List<SalaryComponent>
            {
                new SalaryComponent
                {
                    Id = Guid.NewGuid(),
                    ComponentType = ComponentType.Deduction,
                    Name = "Income Tax (10%)",
                    Amount = 12000m,
                    Percentage = 0.10m,
                    IsTaxable = false,
                    CreatedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SalaryComponent
                {
                    Id = Guid.NewGuid(),
                    ComponentType = ComponentType.Deduction,
                    Name = "Pension Contribution (14%)",
                    Amount = 16800m,
                    Percentage = 0.14m,
                    IsTaxable = false,
                    CreatedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SalaryComponent
                {
                    Id = Guid.NewGuid(),
                    ComponentType = ComponentType.Deduction,
                    Name = "Health Insurance (5.15%)",
                    Amount = 6180m,
                    Percentage = 0.0515m,
                    IsTaxable = false,
                    CreatedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            }
        };

        context.PayrollRuns.Add(completedRun);

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeded successfully with {Count} configurations and {RunCount} payroll runs.", 
            4, 4);
    }
}
