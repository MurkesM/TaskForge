using TaskForge.Api.Models;

namespace TaskForge.Services;

public class TaskService : ITaskService
{
    private readonly List<TaskItem> _tasks = new();
    private int _nextId = 1;

    public IEnumerable<TaskItem> GetAll() => _tasks;

    public TaskItem? Get(int id) => _tasks.FirstOrDefault(t => t.Id == id);

    public TaskItem Create(TaskItem task)
    {
        task.Id = _nextId++;
        _tasks.Add(task);
        return task;
    }

    public TaskItem? Update(int id, TaskItem updated)
    {
        var existing = Get(id);
        if (existing is null) return null;

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.IsComplete = updated.IsComplete;

        return existing;
    }

    public bool Delete(int id)
    {
        var task = Get(id);
        if (task is null) return false;

        _tasks.Remove(task);
        return true;
    }
}
