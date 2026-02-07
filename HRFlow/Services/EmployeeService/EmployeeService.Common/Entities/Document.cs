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

}