namespace Payroll.Application.DTOs;

public class CreatePayrollRunRequest
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public List<CreateSalaryComponentRequest>? AdditionalComponents { get; set; }
}

public class CreateSalaryComponentRequest
{
    public string ComponentType { get; set; } = string.Empty; // "Addition" or "Deduction"
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsTaxable { get; set; }
}
