using TaskForge.Api.Models;

namespace TaskForge.Services;

public interface ITaskService
{
    IEnumerable<TaskItem> GetAll();
    TaskItem? Get(int id);
    TaskItem Create(TaskItem task);
    TaskItem? Update(int id, TaskItem updated);
    bool Delete(int id);
}
