using Microsoft.AspNetCore.Mvc;

namespace Payroll.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new 
        { 
            Status = "Healthy", 
            Service = "Payroll Service",
            Timestamp = DateTime.UtcNow
        });
    }
}
