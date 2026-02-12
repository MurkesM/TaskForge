using TaskForge.Api.Models;
using TaskForge.Repositories;

namespace TaskForge.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;

    public TaskService(ITaskRepository repo)
    {
        _repo = repo;
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
}