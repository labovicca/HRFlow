namespace EmployeeService.Common.Entities;

public class DepartmentHrAssignment
{
    public int Id { get; set; }
    public required string Department { get; set; }
    public required int HrEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
