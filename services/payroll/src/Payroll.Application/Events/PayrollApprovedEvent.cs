namespace Payroll.Application.Events;

/// <summary>
/// Event koji se šalje kada je payroll run odobren.
/// Ostali servisi (npr. Notification) mogu da reaguju na ovaj event.
/// </summary>
public record PayrollApprovedEvent
{
    public Guid PayrollRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal GrossSalary { get; init; }
    public decimal NetSalary { get; init; }
    public decimal TotalDeductions { get; init; }
    public string ApprovedBy { get; init; } = string.Empty;
    public DateTime ApprovedAt { get; init; }
}
