namespace Payroll.Domain.Entities;

public class PayrollConfiguration
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; } // null = global configuration
    
    public decimal BaseSalary { get; set; }
    public decimal TaxRate { get; set; } // e.g., 0.10 = 10%
    public decimal PensionContributionRate { get; set; }
    public decimal HealthInsuranceRate { get; set; }
    public decimal UnemploymentInsuranceRate { get; set; }
    
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
