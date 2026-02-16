using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Models;
using TaskForge.Data;

namespace TaskForge.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(AppDbContext db, ILogger<TaskRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _db.Tasks.AsNoTracking().ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem?> GetByTitleAsync(string title)
    {
        _logger.LogDebug("Querying for task with title {title}", title);
        return await _db.Tasks.FirstOrDefaultAsync(t => t.Title == title);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        var existing = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id);
        if (existing is null)
            return null;

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.IsComplete = task.IsComplete;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return false;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllAsync()
    {
        var beforeDeletionCount = await _db.Tasks.CountAsync();
        var tasks = await _db.Tasks.ToListAsync();
        _db.Tasks.RemoveRange(tasks);
        var deletedCount = await _db.SaveChangesAsync();
        return deletedCount == beforeDeletionCount;
    }
}