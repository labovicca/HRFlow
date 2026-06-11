using EmployeeService.Common.Enums;

namespace EmployeeService.Common.Entities;

public class Document
{
    public required int Id { get; set; }               
    public required int EmployeeId { get; set; }       
    public required string Name { get; set; }         
    public required string FilePath { get; set; }      
    public required DocumentType Type { get; set; }
    public required DocumentStatus Status { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

}
