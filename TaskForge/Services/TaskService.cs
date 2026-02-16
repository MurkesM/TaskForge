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

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<TaskItem?> GetAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        using var scope = _logger.BeginScope("CreateTask {title}", task.Title);

        _logger.LogInformation("Checking for duplicate title");

        var existing = await _repo.GetByTitleAsync(task.Title);
        if (existing is not null)
        {
            _logger.LogWarning("Duplicate title detected");
            throw new DomainException("A task with this title already exists.", 409);
        }

        _logger.LogInformation("Creating task");
        return await _repo.CreateAsync(task);
    }

    public async Task<TaskItem?> UpdateAsync(int id, TaskItem updated)
    {
        // Ensure the ID is set correctly
        updated.Id = id;
        return await _repo.UpdateAsync(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repo.DeleteAsync(id);
    }

    public async Task<bool> DeleteAllAsync()
    {
        return await _repo.DeleteAllAsync();
    }
}