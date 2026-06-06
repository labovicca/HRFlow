using AutoMapper;
using Dapper;
using EmployeeService.Common.Data;
using EmployeeService.Common.DTOs.Document;
using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;

namespace EmployeeService.Common.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly IEmployeeContext _context;
    private readonly IMapper _mapper;

    public DocumentRepository(IEmployeeContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE Id = @Id AND IsDeleted = FALSE";
        var document = await connection.QueryFirstOrDefaultAsync<Document>(sql, new { Id = id });
        return _mapper.Map<DocumentDto?>(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetByEmployeeIdAsync(int employeeId)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE EmployeeId = @EmployeeId AND IsDeleted = FALSE";
        var documents = await connection.QueryAsync<Document>(sql, new { EmployeeId = employeeId });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<IEnumerable<DocumentDto>> GetByEmployeeNumberAsync(string employeeNumber)
    {
        using var connection = _context.GetConnection();
        
        const string employeeSql = "SELECT Id FROM Employee WHERE EmployeeNumber = @EmployeeNumber AND IsDeleted = FALSE";
        var employeeId = await connection.ExecuteScalarAsync<int?>(employeeSql, new { EmployeeNumber = employeeNumber });

        if (employeeId == null)
            return Enumerable.Empty<DocumentDto>();

        return await GetByEmployeeIdAsync(employeeId.Value);
    }

    public async Task<IEnumerable<DocumentDto>> GetByTypeAsync(DocumentType type)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE Type = @Type AND IsDeleted = FALSE";
        var documents = await connection.QueryAsync<Document>(sql, new { Type = (int)type });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<IEnumerable<DocumentDto>> GetExpiringDocumentsAsync(DateTime beforeDate)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE ExpirationDate <= @BeforeDate AND IsDeleted = FALSE";
        var documents = await connection.QueryAsync<Document>(sql, new { BeforeDate = beforeDate });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }
    
    public async Task<IEnumerable<DocumentDto>> GetByStatusAsync(DocumentStatus status)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE Status = @Status AND IsDeleted = FALSE";
        var documents = await connection.QueryAsync<Document>(sql, new { Status = (int)status });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<DocumentDto?> CreateAsync(CreateDocumentDto dto)
    {
        using var connection = _context.GetConnection();

        const string sql = @"
        INSERT INTO Document 
        (EmployeeId, Name, FilePath, Type, Status, ExpirationDate,
         OriginalFileName, ContentType, FileSize, UploadedAt, CreatedAt, CreatedBy)
        VALUES 
        (@EmployeeId, @Name, @FilePath, @Type, @Status, @ExpirationDate,
         @OriginalFileName, @ContentType, @FileSize, @UploadedAt, @CreatedAt, @CreatedBy)
        RETURNING *";

        var document = _mapper.Map<Document>(dto);
        
        if (string.IsNullOrEmpty(dto.Status))
        {
            document.Status = DocumentStatus.Uploaded;
        }

        var createdDocument = await connection.QuerySingleOrDefaultAsync<Document>(sql, new
        {
            document.EmployeeId,
            document.Name,
            document.FilePath,
            Type = (int)document.Type,
            Status = (int)document.Status,
            document.ExpirationDate,
            OriginalFileName = dto.OriginalFileName ?? document.OriginalFileName,
            ContentType = dto.ContentType ?? document.ContentType,
            FileSize = dto.FileSize ?? document.FileSize,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        });
    
        return _mapper.Map<DocumentDto?>(createdDocument);
    }

    public async Task<bool> UpdateAsync(UpdateDocumentDto dto)
    {
        using var connection = _context.GetConnection();

        const string sql = @"
        UPDATE Document
        SET Name = @Name,
            FilePath = @FilePath,
            Type = @Type,
            Status = @Status,
            ExpirationDate = @ExpirationDate,
            UpdatedAt = @UpdatedAt,
            UpdatedBy = @UpdatedBy
        WHERE Id = @Id AND IsDeleted = FALSE";

        var document = _mapper.Map<Document>(dto);

        var affected = await connection.ExecuteAsync(sql, new
        {
            dto.Id,
            document.Name,
            document.FilePath,
            Type = (int)document.Type,
            Status = (int)document.Status,
            document.ExpirationDate,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        });
    
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = """
            UPDATE Document
            SET IsDeleted = TRUE,
                DeletedAt = @DeletedAt,
                DeletedBy = @DeletedBy
            WHERE Id = @Id AND IsDeleted = FALSE
        """;
        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = "system"
        });
        return affected > 0;
    }
    
    public async Task<bool> UpdateStatusAsync(
        int documentId,
        DocumentStatus status)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Document
                               SET Status = @Status,
                                   UpdatedAt = @UpdatedAt,
                                   UpdatedBy = @UpdatedBy
                               WHERE Id = @Id AND IsDeleted = FALSE
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = documentId,
            Status = (int)status,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        });

        return affected > 0;
    }
    
    public async Task<bool> UpdateTypeAsync(int documentId, DocumentType type)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Document
                               SET Type = @Type,
                                   UpdatedAt = @UpdatedAt,
                                   UpdatedBy = @UpdatedBy
                               WHERE Id = @Id AND IsDeleted = FALSE
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = documentId,
            Type = (int)type,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        });

        return affected > 0;
    }
}
