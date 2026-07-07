using Microsoft.AspNetCore.Mvc;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "TableFlow API",
            timestampUtc = DateTime.UtcNow
        });
    }
}