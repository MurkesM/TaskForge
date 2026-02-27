using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Models;
using TaskForge.Dtos;
using TaskForge.Exceptions;

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

    public async Task<CursorResultDto<Notification>> QueryAsync(int userId, NotificationQueryParameters query)
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

        return new CursorResultDto<Notification>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        // Fetch the notification first
        var notification = await _repo.GetByIdAsync(id);
        if (notification is null)
            throw new DomainException("Notification not found.", 404);

        // Enforce ownership
        if (notification.UserId != userId)
            throw new DomainException("You do not have access to this notification.", 403);

        // Mark as read
        await _repo.MarkAsReadAsync(id, userId);
    }
}