namespace Payroll.Application.DTOs;

public class PayrollCalculationResult
{
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    
    public decimal BaseSalary { get; set; }
    public decimal TotalAdditions { get; set; }
    public decimal GrossSalary { get; set; }
    
    public decimal TaxAmount { get; set; }
    public decimal PensionContribution { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal UnemploymentInsurance { get; set; }
    public decimal TotalDeductions { get; set; }
    
    public decimal NetSalary { get; set; }
    
    public List<CalculationBreakdownItem> Breakdown { get; set; } = new();
}

public class CalculationBreakdownItem
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Addition" or "Deduction"
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public string Description { get; set; } = string.Empty;
}
