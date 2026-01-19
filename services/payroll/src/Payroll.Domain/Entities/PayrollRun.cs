using Payroll.Domain.Enums;

namespace Payroll.Domain.Entities;

public class PayrollRun
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public PayrollStatus Status { get; set; }
    
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalAdditions { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public ICollection<SalaryComponent> Components { get; set; } = new List<SalaryComponent>();
    public Payslip? Payslip { get; set; }
}
