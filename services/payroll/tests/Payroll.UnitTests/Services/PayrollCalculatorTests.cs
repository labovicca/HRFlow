using FluentAssertions;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using Payroll.Infrastructure.Services;

namespace Payroll.UnitTests.Services;

public class PayrollCalculatorTests
{
    private readonly PayrollCalculator _calculator;

    public PayrollCalculatorTests()
    {
        _calculator = new PayrollCalculator();
    }

    private static PayrollConfiguration CreateDefaultConfiguration(decimal baseSalary = 1000m)
    {
        return new PayrollConfiguration
        {
            Id = Guid.NewGuid(),
            BaseSalary = baseSalary,
            TaxRate = 0.10m,                    // 10%
            PensionContributionRate = 0.14m,     // 14%
            HealthInsuranceRate = 0.0515m,       // 5.15%
            UnemploymentInsuranceRate = 0.0075m, // 0.75%
            EffectiveFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    private static PayrollRun CreateDefaultPayrollRun()
    {
        return new PayrollRun
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            Year = 2024,
            Month = 1,
            Status = PayrollStatus.Draft,
            Components = new List<SalaryComponent>()
        };
    }

    [Fact]
    public async Task CalculateAsync_WithBaseSalaryOnly_ShouldCalculateCorrectly()
    {
        // Arrange
        var config = CreateDefaultConfiguration(1000m);
        var payrollRun = CreateDefaultPayrollRun();

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        result.BaseSalary.Should().Be(1000m);
        result.GrossSalary.Should().Be(1000m); // no additions
        result.TotalAdditions.Should().Be(0m);

        // Tax: 1000 * 0.10 = 100
        result.TaxAmount.Should().Be(100m);
        // Pension: 1000 * 0.14 = 140
        result.PensionContribution.Should().Be(140m);
        // Health: 1000 * 0.0515 = 51.5
        result.HealthInsurance.Should().Be(51.5m);
        // Unemployment: 1000 * 0.0075 = 7.5
        result.UnemploymentInsurance.Should().Be(7.5m);

        result.TotalDeductions.Should().Be(100m + 140m + 51.5m + 7.5m); // 299
        result.NetSalary.Should().Be(1000m - 299m); // 701
    }

    [Fact]
    public async Task CalculateAsync_WithTaxableAddition_ShouldIncludeInTaxableIncome()
    {
        // Arrange
        var config = CreateDefaultConfiguration(1000m);
        var payrollRun = CreateDefaultPayrollRun();
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Addition,
            Name = "Bonus",
            Amount = 200m,
            IsTaxable = true
        });

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        result.TotalAdditions.Should().Be(200m);
        result.GrossSalary.Should().Be(1200m); // 1000 + 200

        // Tax: (1000 + 200) * 0.10 = 120 (both base and taxable bonus are taxed)
        result.TaxAmount.Should().Be(120m);
        // Pension: 1200 * 0.14 = 168
        result.PensionContribution.Should().Be(168m);
    }

    [Fact]
    public async Task CalculateAsync_WithNonTaxableAddition_ShouldNotIncludeInTaxableIncome()
    {
        // Arrange
        var config = CreateDefaultConfiguration(1000m);
        var payrollRun = CreateDefaultPayrollRun();
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Addition,
            Name = "Putni troskovi",
            Amount = 150m,
            IsTaxable = false
        });

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        result.TotalAdditions.Should().Be(150m);
        result.GrossSalary.Should().Be(1150m); // 1000 + 150

        // Tax should only be on base salary (non-taxable not included)
        result.TaxAmount.Should().Be(100m); // 1000 * 0.10
    }

    [Fact]
    public async Task CalculateAsync_WithPercentageAddition_ShouldCalculateBasedOnBaseSalary()
    {
        // Arrange
        var config = CreateDefaultConfiguration(2000m);
        var payrollRun = CreateDefaultPayrollRun();
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Addition,
            Name = "Performance Bonus",
            Amount = 0m,
            Percentage = 0.15m, // 15% of base salary
            IsTaxable = true
        });

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        // 15% of 2000 = 300
        result.TotalAdditions.Should().Be(300m);
        result.GrossSalary.Should().Be(2300m); // 2000 + 300
    }

    [Fact]
    public async Task CalculateAsync_WithDeductionComponent_ShouldSubtractFromNet()
    {
        // Arrange
        var config = CreateDefaultConfiguration(1000m);
        var payrollRun = CreateDefaultPayrollRun();
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Deduction,
            Name = "Kredit",
            Amount = 50m,
            IsTaxable = false
        });

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        var standardDeductions = 100m + 140m + 51.5m + 7.5m; // 299
        result.TotalDeductions.Should().Be(standardDeductions + 50m); // 349
        result.NetSalary.Should().Be(1000m - 349m); // 651
    }

    [Fact]
    public async Task CalculateAsync_WithPercentageDeduction_ShouldCalculateBasedOnGross()
    {
        // Arrange
        var config = CreateDefaultConfiguration(1000m);
        var payrollRun = CreateDefaultPayrollRun();
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Deduction,
            Name = "Sindikalni prilog",
            Amount = 0m,
            Percentage = 0.02m, // 2% of gross
            IsTaxable = false
        });

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        // Deduction: 2% of 1000 (gross) = 20
        var standardDeductions = 100m + 140m + 51.5m + 7.5m; // 299
        result.TotalDeductions.Should().Be(standardDeductions + 20m); // 319
    }

    [Fact]
    public async Task CalculateAsync_ShouldReturnCorrectBreakdown()
    {
        // Arrange
        var config = CreateDefaultConfiguration(1000m);
        var payrollRun = CreateDefaultPayrollRun();

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        result.Breakdown.Should().NotBeEmpty();
        result.Breakdown.Should().Contain(b => b.Name == "Osnovna plata" && b.Type == "Base");
        result.Breakdown.Should().Contain(b => b.Name == "Bruto plata" && b.Type == "Gross");
        result.Breakdown.Should().Contain(b => b.Name == "Porez na dohodak" && b.Type == "Deduction");
        result.Breakdown.Should().Contain(b => b.Name == "Doprinos za PIO" && b.Type == "Deduction");
        result.Breakdown.Should().Contain(b => b.Name == "Doprinos za zdravstveno" && b.Type == "Deduction");
        result.Breakdown.Should().Contain(b => b.Name == "Doprinos za nezaposlenost" && b.Type == "Deduction");
        result.Breakdown.Should().Contain(b => b.Name == "Neto plata" && b.Type == "Net");
    }

    [Fact]
    public async Task CalculateAsync_WithMultipleComponents_ShouldCalculateAllCorrectly()
    {
        // Arrange
        var config = CreateDefaultConfiguration(3000m);
        var payrollRun = CreateDefaultPayrollRun();
        
        // Taxable bonus
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Addition,
            Name = "Bonus",
            Amount = 500m,
            IsTaxable = true
        });
        
        // Non-taxable travel expenses
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Addition,
            Name = "Putni troskovi",
            Amount = 200m,
            IsTaxable = false
        });
        
        // Loan deduction
        payrollRun.Components.Add(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.Deduction,
            Name = "Kredit",
            Amount = 100m,
            IsTaxable = false
        });

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        result.TotalAdditions.Should().Be(700m); // 500 + 200
        result.GrossSalary.Should().Be(3700m); // 3000 + 700
        
        // Tax on taxable income: (3000 + 500) * 0.10 = 350
        result.TaxAmount.Should().Be(350m);
        
        result.PayrollRunId.Should().Be(payrollRun.Id);
        result.EmployeeId.Should().Be(payrollRun.EmployeeId);
        result.Year.Should().Be(2024);
        result.Month.Should().Be(1);
    }

    [Fact]
    public async Task CalculateAsync_WithZeroRates_ShouldHaveNoDeductions()
    {
        // Arrange
        var config = new PayrollConfiguration
        {
            Id = Guid.NewGuid(),
            BaseSalary = 1000m,
            TaxRate = 0m,
            PensionContributionRate = 0m,
            HealthInsuranceRate = 0m,
            UnemploymentInsuranceRate = 0m,
            EffectiveFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var payrollRun = CreateDefaultPayrollRun();

        // Act
        var result = await _calculator.CalculateAsync(payrollRun, config);

        // Assert
        result.TotalDeductions.Should().Be(0m);
        result.NetSalary.Should().Be(1000m);
        result.GrossSalary.Should().Be(result.NetSalary);
    }
}
