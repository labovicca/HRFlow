namespace Payroll.Application.Events;

/// <summary>
/// Event koji se šalje kada je payroll run kalkulisan.
/// HR može da vidi da je obračun spreman za pregled.
/// </summary>
public record PayrollCalculatedEvent
{
    public Guid PayrollRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal GrossSalary { get; init; }
    public decimal NetSalary { get; init; }
    public decimal TotalDeductions { get; init; }
    public DateTime CalculatedAt { get; init; }
}
