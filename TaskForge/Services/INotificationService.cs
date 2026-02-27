using TaskForge.Api.Models;
using TaskForge.Dtos;

public interface INotificationService
{
    Task NotifyAsync(Notification notification);
    Task<CursorResult<Notification>> QueryAsync(int userId, NotificationQueryParameters query);
    Task MarkAsReadAsync(int id, int userId);
}
