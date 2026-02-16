using TaskForge.Api.Models;

namespace TaskForge.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem?> GetByTitleAsync(string title);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllAsync();
}
