using AutoMapper;
using EmployeeService.Common.Data;
using EmployeeService.Common.DTOs.Document;
using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Common.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly EmployeeDbContext _context;
    private readonly IMapper _mapper;

    public DocumentRepository(EmployeeDbContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var document = await ActiveDocuments()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        return _mapper.Map<DocumentDto?>(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var documents = await ActiveDocuments()
            .AsNoTracking()
            .Where(d => d.EmployeeId == employeeId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<IEnumerable<DocumentDto>> GetByEmployeeNumberAsync(string employeeNumber)
    {
        var employeeId = await _context.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeNumber == employeeNumber && !e.IsDeleted)
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();

        if (employeeId == null)
        {
            return Enumerable.Empty<DocumentDto>();
        }

        return await GetByEmployeeIdAsync(employeeId.Value);
    }

    public async Task<IEnumerable<DocumentDto>> GetByTypeAsync(DocumentType type)
    {
        var documents = await ActiveDocuments()
            .AsNoTracking()
            .Where(d => d.Type == type)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<IEnumerable<DocumentDto>> GetExpiringDocumentsAsync(DateTime beforeDate)
    {
        var documents = await ActiveDocuments()
            .AsNoTracking()
            .Where(d => d.ExpirationDate <= beforeDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<IEnumerable<DocumentDto>> GetByStatusAsync(DocumentStatus status)
    {
        var documents = await ActiveDocuments()
            .AsNoTracking()
            .Where(d => d.Status == status)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<DocumentDto?> CreateAsync(CreateDocumentDto dto)
    {
        var document = _mapper.Map<Document>(dto);

        if (string.IsNullOrEmpty(dto.Status))
        {
            document.Status = DocumentStatus.Uploaded;
        }

        document.OriginalFileName = dto.OriginalFileName ?? document.OriginalFileName;
        document.ContentType = dto.ContentType ?? document.ContentType;
        document.FileSize = dto.FileSize ?? document.FileSize;
        document.UploadedAt = DateTime.UtcNow;
        document.CreatedAt = DateTime.UtcNow;
        document.CreatedBy = "system";
        document.IsDeleted = false;

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return _mapper.Map<DocumentDto?>(document);
    }

    public async Task<bool> UpdateAsync(UpdateDocumentDto dto)
    {
        var document = await ActiveDocuments().FirstOrDefaultAsync(d => d.Id == dto.Id);
        if (document == null)
        {
            return false;
        }

        _mapper.Map(dto, document);
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var document = await ActiveDocuments().FirstOrDefaultAsync(d => d.Id == id);
        if (document == null)
        {
            return false;
        }

        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;
        document.DeletedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateStatusAsync(int documentId, DocumentStatus status)
    {
        var document = await ActiveDocuments().FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null)
        {
            return false;
        }

        document.Status = status;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateTypeAsync(int documentId, DocumentType type)
    {
        var document = await ActiveDocuments().FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null)
        {
            return false;
        }

        document.Type = type;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    private IQueryable<Document> ActiveDocuments()
    {
        return _context.Documents.Where(d => !d.IsDeleted);
    }
}
