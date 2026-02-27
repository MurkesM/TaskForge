using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Models;
using TaskForge.Dtos;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repo;

    public AuditLogService(IAuditLogRepository repo)
    {
        _repo = repo;
    }

    public Task LogAsync(AuditLog log)
    {
        return _repo.AddAsync(log);
    }

    public async Task<CursorResultDto<AuditLog>> QueryAsync(int userId, AuditQueryParameters query)
    {
        // Enforce ownership at the service layer
        var q = _repo.Query()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Id);

        if (query.AfterId.HasValue)
            q = (IOrderedQueryable<AuditLog>)q.Where(a => a.Id < query.AfterId.Value);

        var items = await q.Take(query.Limit + 1).ToListAsync();

        bool hasMore = items.Count > query.Limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        int? nextCursor = hasMore ? items.Last().Id : null;

        return new CursorResultDto<AuditLog>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}