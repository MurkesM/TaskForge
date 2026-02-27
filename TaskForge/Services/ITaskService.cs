using TaskForge.Api.Models;
using TaskForge.Dtos;

namespace TaskForge.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllAsync(int userId, string role);
    Task<TaskItem?> GetAsync(int id, int userId, string role);
    Task<CursorResult<TaskItem>> QueryAsync(TaskQueryParameters query, int userId, string role);
    Task<TaskItem> CreateAsync(TaskItem task, int userId);
    Task<TaskItem?> UpdateAsync(int id, TaskItem updated, int userId, string role);
    Task<bool> DeleteAsync(int id, int userId, string role);
    Task<bool> DeleteAllAsync(int userID, string role);
}