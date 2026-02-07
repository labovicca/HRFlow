using System.ComponentModel.DataAnnotations;

namespace EmployeeService.Common.DTOs.Document;

public class BaseDocumentDto
{
    [Required(ErrorMessage = "Employee ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Employee ID must be greater than 0")]
    public int EmployeeId { get; set; }       

    [Required(ErrorMessage = "Document name is required")]
    [StringLength(200, ErrorMessage = "Document name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;          

    [Required(ErrorMessage = "File path is required")]
    [StringLength(500, ErrorMessage = "File path cannot exceed 500 characters")]
    public string FilePath { get; set; } = string.Empty;      

    public string? Type { get; set; }
    
    public string? Status { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? ExpirationDate { get; set; }
}