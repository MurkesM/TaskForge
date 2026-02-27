using TaskForge.Api.Models;
using TaskForge.Dtos;

public interface INotificationService
{
    Task NotifyAsync(Notification notification);
    Task<CursorResultDto<Notification>> QueryAsync(int userId, NotificationQueryParameters query);
    Task MarkAsReadAsync(int id, int userId);
}
