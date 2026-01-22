namespace Payroll.Application.DTOs;

public class PayrollConfigurationDto
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsGlobal => EmployeeId == null;
    
    public decimal BaseSalary { get; set; }
    public decimal TaxRate { get; set; }
    public decimal PensionContributionRate { get; set; }
    public decimal HealthInsuranceRate { get; set; }
    public decimal UnemploymentInsuranceRate { get; set; }
    
    // Calculated total deduction rate
    public decimal TotalDeductionRate => TaxRate + PensionContributionRate + HealthInsuranceRate + UnemploymentInsuranceRate;
    
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive => EffectiveTo == null || EffectiveTo > DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
