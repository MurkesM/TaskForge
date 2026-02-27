using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Models;
using TaskForge.Data;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Notification> Query()
    {
        return _context.Notifications.AsQueryable();
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        var n = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (n is null)
            return;

        n.IsRead = true;
        await _context.SaveChangesAsync();
    }
}