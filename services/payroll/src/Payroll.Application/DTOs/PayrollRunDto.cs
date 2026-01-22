using Payroll.Domain.Enums;

namespace Payroll.Application.DTOs;

public class PayrollRunDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public PayrollStatus Status { get; set; }
    public string StatusName => Status.ToString();
    
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalAdditions { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    
    public List<SalaryComponentDto> Components { get; set; } = new();
}

public class SalaryComponentDto
{
    public Guid Id { get; set; }
    public string ComponentType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsTaxable { get; set; }
}

public class PayrollRunListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string Period => $"{Year}-{Month:D2}";
    public PayrollStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public DateTime CreatedAt { get; set; }
}
