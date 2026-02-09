using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Commands.PayrollRuns;
using Payroll.Application.DTOs;
using Payroll.Application.Queries.PayrollRuns;
using Payroll.Domain.Enums;

namespace Payroll.API.Controllers;

[ApiController]
[Route("api/payroll/runs")]
public class PayrollRunsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollRunsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Dohvati sve payroll run-ove sa opcionalnim filterima
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PayrollRunListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PayrollRunListDto>>> GetAll(
        [FromQuery] Guid? employeeId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] PayrollStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new GetPayrollRunsQuery(employeeId, year, month, status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Dohvati payroll run po ID-u
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PayrollRunDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayrollRunDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPayrollRunByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
        {
            return NotFound(new { message = $"Payroll run with ID {id} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Kreiraj novi payroll run
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PayrollRunDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PayrollRunDto>> Create(
        [FromBody] CreatePayrollRunRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Get actual user from JWT token
        var createdBy = "system";
        
        var command = new CreatePayrollRunCommand(request, createdBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Izvrši obračun plate za draft payroll run
    /// </summary>
    [HttpPost("{id:guid}/calculate")]
    [ProducesResponseType(typeof(PayrollCalculationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayrollCalculationResult>> Calculate(
        Guid id,
        CancellationToken cancellationToken)
    {
        // TODO: Get actual user from JWT token
        var updatedBy = "system";
        
        var command = new CalculatePayrollCommand(id, updatedBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Odobri obračunati payroll run
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(PayrollRunDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayrollRunDto>> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        // TODO: Get actual user from JWT token
        var approvedBy = "system";
        
        var command = new ApprovePayrollRunCommand(id, approvedBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Generiši payslip PDF za payroll run
    /// </summary>
    [HttpPost("{id:guid}/payslip")]
    [ProducesResponseType(typeof(PayslipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayslipDto>> GeneratePayslip(
        Guid id,
        CancellationToken cancellationToken)
    {
        var generatedBy = "system";
        var command = new GeneratePayslipCommand(id, generatedBy);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Preuzmi payslip PDF za payroll run
    /// </summary>
    [HttpGet("{id:guid}/payslip/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPayslip(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new DownloadPayslipQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { message = "Payslip nije dostupan za ovaj payroll run" });
        }

        return File(result.FileBytes, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Obriši draft ili cancelled payroll run
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePayrollRunCommand(id);
        await _mediator.Send(command, cancellationToken);
        
        return NoContent();
    }
}
