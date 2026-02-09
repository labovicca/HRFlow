namespace Payroll.Application.Events;

/// <summary>
/// Event koji se šalje kada je payslip (platni listić) generisan.
/// Notification servis može da pošalje mejl zaposlenom sa payslip-om.
/// </summary>
public record PayslipGeneratedEvent
{
    public Guid PayslipId { get; init; }
    public Guid PayrollRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
}
