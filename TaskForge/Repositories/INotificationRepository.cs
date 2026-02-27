using TaskForge.Api.Models;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    IQueryable<Notification> Query();
    Task MarkAsReadAsync(int id, int userId);
}