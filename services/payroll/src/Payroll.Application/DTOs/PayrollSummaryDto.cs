namespace Payroll.Application.DTOs;

/// <summary>
/// Monthly payroll summary report showing aggregated payroll data for a period.
/// </summary>
public class PayrollSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    
    // Counts
    public int TotalPayrollRuns { get; set; }
    public int DraftCount { get; set; }
    public int CalculatedCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PaidCount { get; set; }
    public int CancelledCount { get; set; }
    
    // Financial Summary
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalAdditions { get; set; }
    
    // Averages
    public decimal AverageGrossSalary { get; set; }
    public decimal AverageNetSalary { get; set; }
    
    // Min/Max
    public decimal MinNetSalary { get; set; }
    public decimal MaxNetSalary { get; set; }
    
    // Individual employee details for the report
    public List<PayrollRunListDto> PayrollRuns { get; set; } = new();
}
