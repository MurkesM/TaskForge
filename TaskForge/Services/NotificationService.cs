using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Models;
using TaskForge.Dtos;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public Task NotifyAsync(Notification notification)
    {
        return _repo.AddAsync(notification);
    }

    public async Task<CursorResult<Notification>> QueryAsync(int userId, NotificationQueryParameters query)
    {
        var q = _repo.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Id);

        if (query.AfterId.HasValue)
            q = (IOrderedQueryable<Notification>)q.Where(n => n.Id < query.AfterId.Value);

        var items = await q.Take(query.Limit + 1).ToListAsync();

        bool hasMore = items.Count > query.Limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        int? nextCursor = hasMore ? items.Last().Id : null;

        return new CursorResult<Notification>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    public Task MarkAsReadAsync(int id, int userId)
    {
        return _repo.MarkAsReadAsync(id, userId);
    }
}