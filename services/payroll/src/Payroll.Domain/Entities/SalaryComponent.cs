using Payroll.Domain.Enums;

namespace Payroll.Domain.Entities;

public class SalaryComponent
{
    public Guid Id { get; set; }
    public Guid PayrollRunId { get; set; }
    public ComponentType ComponentType { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsTaxable { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public PayrollRun PayrollRun { get; set; } = null!;
}
