using EmployeeService.Common.DTOs.Document;
using EmployeeService.Common.Enums;
using EmployeeService.Common.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentRepository repository, ILogger<DocumentsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("{id}")]
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
    public async Task<ActionResult> Create([FromBody] CreateDocumentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repository.CreateAsync(dto);
            if (!result)
                return BadRequest("Failed to create document");

            return CreatedAtAction(nameof(GetById), new { id = dto.EmployeeId }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
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
}