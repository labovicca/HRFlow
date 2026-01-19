namespace Payroll.Domain.Entities;

public class Payslip
{
    public Guid Id { get; set; }
    public Guid PayrollRunId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    
    // Navigation property
    public PayrollRun PayrollRun { get; set; } = null!;
}
