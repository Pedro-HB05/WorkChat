using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkChat.Data;

namespace WorkChat.Controllers;

[ApiController]
[AllowAnonymous]
[Route("health")]
public sealed class HealthController(WorkChatDbContext db) : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "healthy", utc = DateTime.UtcNow });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken ct)
    {
        if (await db.Database.CanConnectAsync(ct)) return Ok(new { status = "healthy", database = "connected", utc = DateTime.UtcNow });
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails { Status = 503, Title = "Servico indisponivel", Detail = "Nao foi possivel conectar ao banco de dados." });
    }
}
