using Microsoft.AspNetCore.Mvc;

namespace ShopFlow.ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new
        {
            status = "Healthy",
            service = "ShopFlow API Gateway",
            timestamp = DateTime.UtcNow
        });
}