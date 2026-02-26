using TaskForge.Api.Models;
using TaskForge.Exceptions;
using TaskForge.Repositories;

namespace TaskForge.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository repo, ILogger<TaskService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // GET ALL — user sees only their tasks, admin sees all
    public async Task<IEnumerable<TaskItem>> GetAllAsync(int userId, string role)
    {
        var all = await _repo.GetAllAsync();

        if (role == "Admin")
            return all;

        return all.Where(t => t.UserId == userId);
    }

    // GET ONE — enforce ownership
    public async Task<TaskItem?> GetAsync(int id, int userId, string role)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task is null)
            return null;

        if (task.UserId != userId && role != "Admin")
            return null;

        return task;
    }

    // CREATE — assign ownership
    public async Task<TaskItem> CreateAsync(TaskItem task, int userId)
    {
        using var scope = _logger.BeginScope("CreateTask {title}", task.Title);

        var existing = await _repo.GetByTitleAsync(task.Title);
        if (existing is not null)
            throw new DomainException("A task with this title already exists.", 409);

        task.UserId = userId;

        return await _repo.CreateAsync(task);
    }

    // UPDATE — enforce ownership
    public async Task<TaskItem?> UpdateAsync(int id, TaskItem updated, int userId, string role)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return null;

        if (existing.UserId != userId && role != "Admin")
            return null;

        updated.Id = id;
        updated.UserId = existing.UserId;

        return await _repo.UpdateAsync(updated);
    }

    // DELETE — enforce ownership
    public async Task<bool> DeleteAsync(int id, int userId, string role)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return false;

        if (existing.UserId != userId && role != "Admin")
            return false;

        return await _repo.DeleteAsync(id);
    }

    // DELETE ALL — admin only
    public async Task<bool> DeleteAllAsync(string role)
    {
        if (role != "Admin")
            return false;

        return await _repo.DeleteAllAsync();
    }
}