namespace Payroll.Application.Events;

/// <summary>
/// Event published when a payroll run is marked as Paid.
/// The Notification Service can use this to inform the employee that their salary has been paid.
/// </summary>
public record PayrollPaidEvent
{
    public Guid PayrollRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal NetSalary { get; init; }
    public string PaidBy { get; init; } = string.Empty;
    public DateTime PaidAt { get; init; }
}
