namespace EmployeeService.Common.DTOs.Document;

public class DocumentDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; }
    public string FilePath { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public DateTime? ExpirationDate { get; set; }
}