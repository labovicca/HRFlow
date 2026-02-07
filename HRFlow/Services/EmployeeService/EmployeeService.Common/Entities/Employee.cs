using EmployeeService.Common.Enums;

namespace EmployeeService.Common.Entities;
public class Employee
{
    public required int Id { get; set; }
    public required string EmployeeNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string WorkEmail { get; set; }
    public required string PersonalEmail { get; set; }
    public required string PhoneNumber { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public required string JMBG { get; set; }
    
    public required string Department { get; set; }
    public required string Position { get; set; }
    public int? ManagerId { get; set; }
    
    public required EmploymentType EmploymentType { get; set; }
    public required EmploymentStatus EmploymentStatus { get; set; }
    public required DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
}