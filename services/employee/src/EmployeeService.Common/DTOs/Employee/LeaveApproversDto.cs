namespace EmployeeService.Common.DTOs.Employee;

public class LeaveApproversDto
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public string ManagerEmployeeNumber { get; set; } = string.Empty;
    public int HrEmployeeId { get; set; }
    public string HrEmployeeNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
