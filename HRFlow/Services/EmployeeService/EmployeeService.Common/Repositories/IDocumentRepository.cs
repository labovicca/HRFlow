using EmployeeService.Common.DTOs.Document;
using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;

namespace EmployeeService.Common.Repositories;

public interface IDocumentRepository
{
    Task<DocumentDto?> GetByIdAsync(int id);
    Task<IEnumerable<DocumentDto>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<DocumentDto>> GetByEmployeeNumberAsync(string employeeNumber);
    Task<IEnumerable<DocumentDto>> GetByTypeAsync(DocumentType type);
    Task<IEnumerable<DocumentDto>> GetExpiringDocumentsAsync(DateTime beforeDate);
    Task<IEnumerable<DocumentDto>> GetByStatusAsync(DocumentStatus status);

    Task<bool> CreateAsync(CreateDocumentDto document);
    Task<bool> UpdateAsync(UpdateDocumentDto document);
    Task<bool> DeleteAsync(int id);
    
    Task<bool> UpdateStatusAsync(int documentId, DocumentStatus status);
    Task<bool> UpdateTypeAsync(int documentId, DocumentType type);

}