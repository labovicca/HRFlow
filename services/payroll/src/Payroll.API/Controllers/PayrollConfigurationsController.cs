using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Commands.PayrollConfigurations;
using Payroll.Application.DTOs;
using Payroll.Application.Queries.PayrollConfigurations;

namespace Payroll.API.Controllers;

[ApiController]
[Route("api/payroll/configurations")]
public class PayrollConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollConfigurationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all payroll configurations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PayrollConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PayrollConfigurationDto>>> GetAll(
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var query = new GetPayrollConfigurationsQuery(employeeId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get payroll configuration by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PayrollConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayrollConfigurationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPayrollConfigurationByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
        {
            return NotFound(new { message = $"Payroll configuration with ID {id} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new payroll configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PayrollConfigurationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PayrollConfigurationDto>> Create(
        [FromBody] CreatePayrollConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Get actual user from JWT token
            var createdBy = "system";
            
            var command = new CreatePayrollConfigurationCommand(request, createdBy);
            var result = await _mediator.Send(command, cancellationToken);
            
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing payroll configuration
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PayrollConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PayrollConfigurationDto>> Update(
        Guid id,
        [FromBody] UpdatePayrollConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Get actual user from JWT token
            var updatedBy = "system";
            
            var command = new UpdatePayrollConfigurationCommand(id, request, updatedBy);
            var result = await _mediator.Send(command, cancellationToken);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a payroll configuration
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeletePayrollConfigurationCommand(id);
            await _mediator.Send(command, cancellationToken);
            
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
