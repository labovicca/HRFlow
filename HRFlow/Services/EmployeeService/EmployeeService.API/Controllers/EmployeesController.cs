using EmployeeService.Common.DTOs.Employee;
using EmployeeService.Common.Enums;
using EmployeeService.Common.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeRepository repository, ILogger<EmployeesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        try
        {
            var employees = await _repository.GetAllAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all employees");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        try
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                return NotFound($"Employee with ID {id} not found");

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("number/{employeeNumber}")]
    public async Task<ActionResult<EmployeeDto>> GetByEmployeeNumber(string employeeNumber)
    {
        try
        {
            var employee = await _repository.GetByEmployeeNumberAsync(employeeNumber);
            if (employee == null)
                return NotFound($"Employee with number {employeeNumber} not found");

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by number: {EmployeeNumber}", employeeNumber);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("department/{department}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByDepartment(string department)
    {
        try
        {
            var employees = await _repository.GetByDepartmentAsync(department);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by department: {Department}", department);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("manager/{managerId}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByManagerId(int managerId)
    {
        try
        {
            var employees = await _repository.GetByManagerIdAsync(managerId);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by manager: {ManagerId}", managerId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByStatus(EmploymentStatus status)
    {
        try
        {
            var employees = await _repository.GetByStatusAsync(status);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by status: {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByType(EmploymentType type)
    {
        try
        {
            var employees = await _repository.GetByTypeAsync(type);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by type: {Type}", type);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdEmployee = await _repository.CreateAsync(dto);
        
            if (createdEmployee == null)
                return BadRequest("Failed to create employee");
            
            return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repository.UpdateAsync(dto);
            if (!result)
                return NotFound($"Employee with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] EmploymentStatus status)
    {
        try
        {
            var result = await _repository.UpdateStatusAsync(id, status);
            if (!result)
                return NotFound($"Employee with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee status: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/type")]
    public async Task<ActionResult> UpdateType(int id, [FromBody] EmploymentType type)
    {
        try
        {
            var result = await _repository.UpdateTypeAsync(id, type);
            if (!result)
                return NotFound($"Employee with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee type: {Id}", id);
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
                return NotFound($"Employee with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPatch("{id}/terminate")]
    public async Task<ActionResult> TerminateEmployee(int id, [FromBody] DateTime terminationDate)
    {
        try
        {
            if (terminationDate > DateTime.UtcNow.Date)
                return BadRequest("Termination date cannot be in the future");
        
            if (terminationDate < new DateTime(1900, 1, 1))
                return BadRequest("Invalid termination date");
        
            var result = await _repository.TerminateAsync(id, terminationDate);
        
            if (!result)
                return NotFound($"Employee with ID {id} not found");
        
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating employee: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}