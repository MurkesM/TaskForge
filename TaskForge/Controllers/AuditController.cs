using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskForge.Data;
using TaskForge.Dtos;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserRole =>
        User.FindFirst(ClaimTypes.Role)!.Value;

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditQueryParameters query)
    {
        if (CurrentUserRole != "Admin")
            return Forbid();

        var q = _context.AuditLogs.OrderBy(l => l.Id).AsQueryable();

        if (query.AfterId.HasValue)
            q = q.Where(l => l.Id > query.AfterId.Value);

        var items = await q.Take(query.Limit + 1).ToListAsync();

        bool hasMore = items.Count > query.Limit;

        if (hasMore)
            items.RemoveAt(items.Count - 1);

        int? nextCursor = hasMore ? items.Last().Id : null;

        return Ok(new
        {
            items,
            nextCursor,
            hasMore
        });
    }
}