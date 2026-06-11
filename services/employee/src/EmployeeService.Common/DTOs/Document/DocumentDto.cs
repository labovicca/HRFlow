namespace EmployeeService.Common.DTOs.Document;

public class DocumentDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Status { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
