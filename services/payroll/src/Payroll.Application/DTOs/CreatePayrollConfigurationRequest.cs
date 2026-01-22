namespace Payroll.Application.DTOs;

public class CreatePayrollConfigurationRequest
{
    public Guid? EmployeeId { get; set; } // null = global configuration
    public decimal BaseSalary { get; set; }
    public decimal TaxRate { get; set; } // e.g., 0.10 = 10%
    public decimal PensionContributionRate { get; set; }
    public decimal HealthInsuranceRate { get; set; }
    public decimal UnemploymentInsuranceRate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class UpdatePayrollConfigurationRequest
{
    public decimal BaseSalary { get; set; }
    public decimal TaxRate { get; set; }
    public decimal PensionContributionRate { get; set; }
    public decimal HealthInsuranceRate { get; set; }
    public decimal UnemploymentInsuranceRate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
