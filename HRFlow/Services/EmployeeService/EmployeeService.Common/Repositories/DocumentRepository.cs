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
        const string sql = "SELECT * FROM Document WHERE Id = @Id";
        var document = await connection.QueryFirstOrDefaultAsync<Document>(sql, new { Id = id });
        return _mapper.Map<DocumentDto?>(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetByEmployeeIdAsync(int employeeId)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE EmployeeId = @EmployeeId";
        var documents = await connection.QueryAsync<Document>(sql, new { EmployeeId = employeeId });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<IEnumerable<DocumentDto>> GetByEmployeeNumberAsync(string employeeNumber)
    {
        using var connection = _context.GetConnection();
        
        const string employeeSql = "SELECT Id FROM Employee WHERE EmployeeNumber = @EmployeeNumber";
        var employeeId = await connection.ExecuteScalarAsync<int?>(employeeSql, new { EmployeeNumber = employeeNumber });

        if (employeeId == null)
            return Enumerable.Empty<DocumentDto>();

        return await GetByEmployeeIdAsync(employeeId.Value);
    }

    public async Task<IEnumerable<DocumentDto>> GetByTypeAsync(DocumentType type)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE Type = @Type";
        var documents = await connection.QueryAsync<Document>(sql, new { Type = (int)type });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<IEnumerable<DocumentDto>> GetExpiringDocumentsAsync(DateTime beforeDate)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE ExpirationDate <= @BeforeDate";
        var documents = await connection.QueryAsync<Document>(sql, new { BeforeDate = beforeDate });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }
    
    public async Task<IEnumerable<DocumentDto>> GetByStatusAsync(DocumentStatus status)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Document WHERE Status = @Status";
        var documents = await connection.QueryAsync<Document>(sql, new { Status = (int)status });
        return _mapper.Map<IEnumerable<DocumentDto>>(documents);
    }

    public async Task<bool> CreateAsync(CreateDocumentDto dto)
    {
        using var connection = _context.GetConnection();

        const string sql = @"
        INSERT INTO Document 
        (EmployeeId, Name, FilePath, Type, Status, ExpirationDate)
        VALUES 
        (@EmployeeId, @Name, @FilePath, @Type, @Status, @ExpirationDate)";

        var document = _mapper.Map<Document>(dto);
        
        if (string.IsNullOrEmpty(dto.Status))
        {
            document.Status = DocumentStatus.Uploaded;
        }

        var affected = await connection.ExecuteAsync(sql, new
        {
            document.EmployeeId,
            document.Name,
            document.FilePath,
            Type = (int)document.Type,
            Status = (int)document.Status,
            document.ExpirationDate
        });
    
        return affected > 0;
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
            ExpirationDate = @ExpirationDate
        WHERE Id = @Id";

        var document = _mapper.Map<Document>(dto);

        var affected = await connection.ExecuteAsync(sql, new
        {
            dto.Id,
            document.Name,
            document.FilePath,
            Type = (int)document.Type,
            Status = (int)document.Status,
            document.ExpirationDate
        });
    
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = "DELETE FROM Document WHERE Id = @Id";
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
    
    public async Task<bool> UpdateStatusAsync(
        int documentId,
        DocumentStatus status)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Document
                               SET Status = @Status
                               WHERE Id = @Id
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = documentId,
            Status = (int)status
        });

        return affected > 0;
    }
    
    public async Task<bool> UpdateTypeAsync(int documentId, DocumentType type)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Document
                               SET Type = @Type
                               WHERE Id = @Id
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = documentId,
            Type = (int)type
        });

        return affected > 0;
    }
}