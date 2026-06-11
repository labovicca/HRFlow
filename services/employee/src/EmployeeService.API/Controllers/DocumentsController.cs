using EmployeeService.Common.DTOs.Document;
using EmployeeService.Common.Enums;
using EmployeeService.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace EmployeeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<DocumentsController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public DocumentsController(
        IDocumentRepository repository,
        IEmployeeRepository employeeRepository,
        ILogger<DocumentsController> logger,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetById(int id)
    {
        try
        {
            var document = await _repository.GetByIdAsync(id);
            if (document == null)
                return NotFound($"Document with ID {id} not found");

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document by ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByEmployeeId(int employeeId)
    {
        try
        {
            var documents = await _repository.GetByEmployeeIdAsync(employeeId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents by employee ID: {EmployeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("employee/number/{employeeNumber}")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByEmployeeNumber(string employeeNumber)
    {
        try
        {
            var documents = await _repository.GetByEmployeeNumberAsync(employeeNumber);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents by employee number: {EmployeeNumber}", employeeNumber);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByType(DocumentType type)
    {
        try
        {
            var documents = await _repository.GetByTypeAsync(type);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents by type: {Type}", type);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByStatus(DocumentStatus status)
    {
        try
        {
            var documents = await _repository.GetByStatusAsync(status);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents by status: {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("expiring/{beforeDate}")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetExpiringDocuments(DateTime beforeDate)
    {
        try
        {
            var documents = await _repository.GetExpiringDocumentsAsync(beforeDate);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring documents before: {BeforeDate}", beforeDate);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentDto>> Create([FromBody] CreateDocumentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdDocument = await _repository.CreateAsync(dto);
            if (createdDocument == null)
                return BadRequest("Failed to create document");

            return CreatedAtAction(nameof(GetById), new { id = createdDocument.Id }, createdDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> Upload(
        [FromForm] int employeeId,
        [FromForm] string name,
        [FromForm] IFormFile file,
        [FromForm] string? type,
        [FromForm] DateTime? expirationDate)
    {
        if (file.Length == 0)
            return BadRequest("File is required");

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Document name is required");

        var validationError = ValidateUploadedFile(file);
        if (validationError != null)
            return BadRequest(validationError);

        if (!await _employeeRepository.ExistsAsync(employeeId))
            return NotFound($"Employee with ID {employeeId} not found");

        var relativePath = await SaveDocumentFileAsync(employeeId, file);
        var originalFileName = GetSafeOriginalFileName(file);

        var dto = new CreateDocumentDto
        {
            EmployeeId = employeeId,
            Name = name,
            FilePath = relativePath,
            Type = type,
            Status = DocumentStatus.Uploaded.ToString(),
            ExpirationDate = expirationDate,
            OriginalFileName = originalFileName,
            ContentType = file.ContentType,
            FileSize = file.Length
        };

        var createdDocument = await _repository.CreateAsync(dto);
        if (createdDocument == null)
        {
            DeleteFileIfExists(relativePath);
            return BadRequest("Failed to create document");
        }

        return CreatedAtAction(nameof(GetById), new { id = createdDocument.Id }, createdDocument);
    }

    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id)
    {
        var document = await _repository.GetByIdAsync(id);
        if (document == null)
            return NotFound($"Document with ID {id} not found");

        var fullPath = GetAbsoluteStoragePath(document.FilePath);
        if (!System.IO.File.Exists(fullPath))
            return NotFound("Document file not found");

        var fileName = string.IsNullOrWhiteSpace(document.OriginalFileName)
            ? Path.GetFileName(fullPath)
            : document.OriginalFileName;
        var contentType = string.IsNullOrWhiteSpace(document.ContentType)
            ? "application/octet-stream"
            : document.ContentType;
        return PhysicalFile(fullPath, contentType, fileName, enableRangeProcessing: true);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDocumentDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repository.UpdateAsync(dto);
            if (!result)
                return NotFound($"Document with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] DocumentStatus status)
    {
        try
        {
            var result = await _repository.UpdateStatusAsync(id, status);
            if (!result)
                return NotFound($"Document with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document status: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/type")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateType(int id, [FromBody] DocumentType type)
    {
        try
        {
            var result = await _repository.UpdateTypeAsync(id, type);
            if (!result)
                return NotFound($"Document with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document type: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
                return NotFound($"Document with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task<string> SaveDocumentFileAsync(int employeeId, IFormFile file)
    {
        var storageRoot = GetStorageRoot();
        var employeeFolder = Path.Combine(storageRoot, employeeId.ToString());
        Directory.CreateDirectory(employeeFolder);

        var originalFileName = GetSafeOriginalFileName(file);
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(employeeFolder, storedFileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return Path.Combine(employeeId.ToString(), storedFileName).Replace('\\', '/');
    }

    private string? ValidateUploadedFile(IFormFile file)
    {
        var maxFileSizeBytes = _configuration.GetValue<long?>("DocumentStorage:MaxFileSizeBytes") ?? 10 * 1024 * 1024;
        if (file.Length > maxFileSizeBytes)
            return $"File size exceeds the limit of {maxFileSizeBytes} bytes";

        var allowedExtensions = _configuration
            .GetSection("DocumentStorage:AllowedExtensions")
            .Get<string[]>() ?? new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

        var allowedContentTypes = _configuration
            .GetSection("DocumentStorage:AllowedContentTypes")
            .Get<string[]>() ?? new[]
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "image/jpeg",
                "image/png"
            };

        var extension = Path.GetExtension(GetSafeOriginalFileName(file));
        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return $"File extension '{extension}' is not allowed";

        if (!allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return $"File content type '{file.ContentType}' is not allowed";

        return null;
    }

    private static string GetSafeOriginalFileName(IFormFile file)
    {
        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Value?.Trim('"')
            ?? file.FileName;
        return Path.GetFileName(fileName);
    }

    private string GetStorageRoot()
    {
        var configuredPath = _configuration.GetValue<string>("DocumentStorage:RootPath") ?? "Storage/Documents";
        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
    }

    private string GetAbsoluteStoragePath(string relativePath)
    {
        var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(GetStorageRoot(), normalizedPath);
    }

    private void DeleteFileIfExists(string relativePath)
    {
        var fullPath = GetAbsoluteStoragePath(relativePath);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
