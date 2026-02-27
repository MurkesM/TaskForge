using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskForge.Models;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] NotificationQueryParameters query)
    {
        var result = await _service.QueryAsync(CurrentUserId, query);
        return Ok(result);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _service.MarkAsReadAsync(id, CurrentUserId);
        return NoContent();
    }
}