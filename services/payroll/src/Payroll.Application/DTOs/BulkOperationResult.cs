namespace Payroll.Application.DTOs;

public class BulkOperationResult
{
    public int TotalProcessed { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public List<BulkOperationError> Errors { get; set; } = new();
}

public class BulkOperationError
{
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
