using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Infrastructure.Services;

public class PayrollCalculator : IPayrollCalculator
{
    public Task<PayrollCalculationResult> CalculateAsync(
        PayrollRun payrollRun,
        PayrollConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var result = new PayrollCalculationResult
        {
            PayrollRunId = payrollRun.Id,
            EmployeeId = payrollRun.EmployeeId,
            Year = payrollRun.Year,
            Month = payrollRun.Month,
            BaseSalary = configuration.BaseSalary
        };

        var breakdown = new List<CalculationBreakdownItem>();

        // Add base salary
        breakdown.Add(new CalculationBreakdownItem
        {
            Name = "Osnovna plata",
            Type = "Base",
            Amount = configuration.BaseSalary,
            Description = "Osnovna mesečna plata"
        });

        // Calculate additions from components
        decimal totalAdditions = 0;
        var additionComponents = payrollRun.Components
            .Where(c => c.ComponentType == ComponentType.Addition)
            .ToList();

        foreach (var addition in additionComponents)
        {
            decimal additionAmount;
            if (addition.Percentage.HasValue)
            {
                additionAmount = configuration.BaseSalary * addition.Percentage.Value;
            }
            else
            {
                additionAmount = addition.Amount;
            }

            totalAdditions += additionAmount;
            breakdown.Add(new CalculationBreakdownItem
            {
                Name = addition.Name,
                Type = "Addition",
                Amount = additionAmount,
                Percentage = addition.Percentage,
                Description = addition.IsTaxable ? "Oporezivi dodatak" : "Neoporezivi dodatak"
            });
        }

        result.TotalAdditions = totalAdditions;

        // Calculate gross salary
        decimal grossSalary = configuration.BaseSalary + totalAdditions;
        result.GrossSalary = grossSalary;

        breakdown.Add(new CalculationBreakdownItem
        {
            Name = "Bruto plata",
            Type = "Gross",
            Amount = grossSalary,
            Description = "Osnovna plata + dodaci"
        });

        // Calculate deductions
        decimal totalDeductions = 0;

        // Tax
        decimal taxableIncome = configuration.BaseSalary + 
            additionComponents.Where(c => c.IsTaxable).Sum(c => c.Percentage.HasValue 
                ? configuration.BaseSalary * c.Percentage.Value 
                : c.Amount);
        
        decimal taxAmount = taxableIncome * configuration.TaxRate;
        result.TaxAmount = taxAmount;
        totalDeductions += taxAmount;

        breakdown.Add(new CalculationBreakdownItem
        {
            Name = "Porez na dohodak",
            Type = "Deduction",
            Amount = taxAmount,
            Percentage = configuration.TaxRate,
            Description = $"Porez {configuration.TaxRate * 100:F2}% na oporezivi prihod"
        });

        // Pension contribution
        decimal pensionContribution = grossSalary * configuration.PensionContributionRate;
        result.PensionContribution = pensionContribution;
        totalDeductions += pensionContribution;

        breakdown.Add(new CalculationBreakdownItem
        {
            Name = "Doprinos za PIO",
            Type = "Deduction",
            Amount = pensionContribution,
            Percentage = configuration.PensionContributionRate,
            Description = $"Penzijsko osiguranje {configuration.PensionContributionRate * 100:F2}%"
        });

        // Health insurance
        decimal healthInsurance = grossSalary * configuration.HealthInsuranceRate;
        result.HealthInsurance = healthInsurance;
        totalDeductions += healthInsurance;

        breakdown.Add(new CalculationBreakdownItem
        {
            Name = "Doprinos za zdravstveno",
            Type = "Deduction",
            Amount = healthInsurance,
            Percentage = configuration.HealthInsuranceRate,
            Description = $"Zdravstveno osiguranje {configuration.HealthInsuranceRate * 100:F2}%"
        });

        // Unemployment insurance
        decimal unemploymentInsurance = grossSalary * configuration.UnemploymentInsuranceRate;
        result.UnemploymentInsurance = unemploymentInsurance;
        totalDeductions += unemploymentInsurance;

        breakdown.Add(new CalculationBreakdownItem
        {
            Name = "Doprinos za nezaposlenost",
            Type = "Deduction",
            Amount = unemploymentInsurance,
            Percentage = configuration.UnemploymentInsuranceRate,
            Description = $"Osiguranje za nezaposlenost {configuration.UnemploymentInsuranceRate * 100:F2}%"
        });

        // Calculate deductions from components
        var deductionComponents = payrollRun.Components
            .Where(c => c.ComponentType == ComponentType.Deduction)
            .ToList();

        foreach (var deduction in deductionComponents)
        {
            decimal deductionAmount;
            if (deduction.Percentage.HasValue)
            {
                deductionAmount = grossSalary * deduction.Percentage.Value;
            }
            else
            {
                deductionAmount = deduction.Amount;
            }

            totalDeductions += deductionAmount;
            breakdown.Add(new CalculationBreakdownItem
            {
                Name = deduction.Name,
                Type = "Deduction",
                Amount = deductionAmount,
                Percentage = deduction.Percentage,
                Description = "Dodatni odbitak"
            });
        }

        result.TotalDeductions = totalDeductions;

        // Calculate net salary
        decimal netSalary = grossSalary - totalDeductions;
        result.NetSalary = netSalary;

        breakdown.Add(new CalculationBreakdownItem
        {
            Name = "Neto plata",
            Type = "Net",
            Amount = netSalary,
            Description = "Bruto plata - odbici"
        });

        result.Breakdown = breakdown;

        return Task.FromResult(result);
    }
}
