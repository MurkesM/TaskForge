using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Models;
using TaskForge.Data;

namespace TaskForge.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public IEnumerable<TaskItem> GetAll()
    {
        return _db.Tasks.AsNoTracking().ToList();
    }

    public TaskItem? Get(int id)
    {
        return _db.Tasks.AsNoTracking().FirstOrDefault(t => t.Id == id);
    }

    public TaskItem Create(TaskItem task)
    {
        _db.Tasks.Add(task);
        _db.SaveChanges();
        return task;
    }

    public TaskItem? Update(int id, TaskItem updated)
    {
        var existing = _db.Tasks.FirstOrDefault(t => t.Id == id);
        if (existing is null)
            return null;

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.IsComplete = updated.IsComplete;

        _db.SaveChanges();
        return existing;
    }

    public bool Delete(int id)
    {
        var task = _db.Tasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
            return false;

        _db.Tasks.Remove(task);
        _db.SaveChanges();
        return true;
    }
}
