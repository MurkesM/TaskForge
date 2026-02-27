using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskForge.Dtos;

namespace TaskForge.Controllers;

[Authorize]
[ApiController]
[Route("audit")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Query([FromQuery] AuditQueryParameters query)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _service.QueryAsync(userId, query);

        return Ok(result);
    }
}