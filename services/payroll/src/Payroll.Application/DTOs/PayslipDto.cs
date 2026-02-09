namespace Payroll.Application.DTOs;

public class PayslipDto
{
    public Guid Id { get; set; }
    public Guid PayrollRunId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}
