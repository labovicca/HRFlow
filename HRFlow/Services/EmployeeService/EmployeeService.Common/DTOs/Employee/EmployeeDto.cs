using EmployeeService.Common.Enums;

namespace EmployeeService.Common.DTOs.Employee;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string WorkEmail { get; set; }
    public string Department { get; set; }
    public string Position { get; set; }

    public EmploymentType EmploymentType { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    
    public string PersonalEmail { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string JMBG { get; set; }
    public int? ManagerId { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public DateTime? TerminationDate { get; set; }
}