using System.ComponentModel.DataAnnotations;
using EmployeeService.Common.Enums;
namespace EmployeeService.Common.DTOs.Employee;

public class BaseEmployeeDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Work email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string WorkEmail { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string PersonalEmail { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "JMBG is required")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG must be exactly 13 digits")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "JMBG must contain only digits")]
    public string JMBG { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position is required")]
    public string Position { get; set; } = string.Empty;

    public int? ManagerId { get; set; }

    [Required(ErrorMessage = "Employment type is required")]
    public EmploymentType EmploymentType { get; set; }

    [Required(ErrorMessage = "Employment status is required")]
    public EmploymentStatus EmploymentStatus { get; set; }

    [Required(ErrorMessage = "Hire date is required")]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ProbationEndDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? TerminationDate { get; set; }
}