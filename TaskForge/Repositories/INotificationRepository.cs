using TaskForge.Api.Models;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    IQueryable<Notification> Query();
    Task<Notification?> GetByIdAsync(int id);
    Task MarkAsReadAsync(int id, int userId);
}