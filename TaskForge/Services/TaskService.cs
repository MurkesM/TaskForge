using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskForge.Api.Models;
using TaskForge.Dtos;
using TaskForge.Exceptions;
using TaskForge.Models;
using TaskForge.Repositories;

namespace TaskForge.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ILogger<TaskService> _logger;
    private readonly IAuditLogService _audit;
    private readonly INotificationService _notifications;

    public TaskService(
        ITaskRepository repo,
        ILogger<TaskService> logger,
        IAuditLogService audit,
        INotificationService notifications)
    {
        _repo = repo;
        _logger = logger;
        _audit = audit;
        _notifications = notifications;
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
            throw new DomainException("Task not found.", 404);

        if (task.UserId != userId && role != "Admin")
            throw new DomainException("You do not have access to this task.", 403);

        return task;
    }

    public async Task<CursorResultDto<TaskItem>> QueryAsync(TaskQueryParameters query, int userId, string role)
    {
        var q = _repo.Query();

        if (role != "Admin")
            q = q.Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(t =>
                t.Title.ToLower().Contains(search) ||
                (t.Description != null && t.Description.ToLower().Contains(search))
            );
        }

        if (query.IsComplete.HasValue)
            q = q.Where(t => t.IsComplete == query.IsComplete.Value);

        q = query.SortBy?.ToLower() switch
        {
            "title" => query.Description ? q.OrderByDescending(t => t.Title).ThenByDescending(t => t.Id)
                                      : q.OrderBy(t => t.Title).ThenBy(t => t.Id),

            "iscomplete" => query.Description ? q.OrderByDescending(t => t.IsComplete).ThenByDescending(t => t.Id)
                                      : q.OrderBy(t => t.IsComplete).ThenBy(t => t.Id),

            _ => query.Description ? q.OrderByDescending(t => t.Id)
                                      : q.OrderBy(t => t.Id)
        };

        if (query.AfterId.HasValue)
            q = q.Where(t => t.Id > query.AfterId.Value);

        var items = await q.Take(query.Limit + 1).ToListAsync();

        bool hasMore = items.Count > query.Limit;

        if (hasMore)
            items.RemoveAt(items.Count - 1);

        int? nextCursor = hasMore ? items.Last().Id : null;

        return new CursorResultDto<TaskItem>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    // CREATE — assign ownership + audit + notify
    public async Task<TaskItem> CreateAsync(TaskItem task, int userId)
    {
        using var scope = _logger.BeginScope("CreateTask {title}", task.Title);

        var existing = await _repo.GetByTitleAsync(task.Title);
        if (existing is not null)
            throw new DomainException("A task with this title already exists.", 409);

        task.UserId = userId;

        var created = await _repo.CreateAsync(task);

        await _audit.LogAsync(new AuditLog
        {
            UserId = userId,
            Action = "TaskCreated",
            EntityType = "Task",
            EntityId = created.Id,
            Metadata = JsonSerializer.Serialize(new { created.Title })
        });

        await _notifications.NotifyAsync(new Notification
        {
            UserId = userId,
            Type = "TaskCreated",
            Message = $"Your task '{created.Title}' was created.",
            EntityType = "Task",
            EntityId = created.Id
        });

        return created;
    }

    // UPDATE — enforce ownership + audit + notify (including completion)
    public async Task<TaskItem?> UpdateAsync(int id, TaskItem updated, int userId, string role)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            throw new DomainException("Task not found.", 404);

        if (existing.UserId != userId && role != "Admin")
            throw new DomainException("You do not have access to this task.", 403);

        var oldTitle = existing.Title;
        var oldDescription = existing.Description;
        var wasComplete = existing.IsComplete;

        updated.Id = id;
        updated.UserId = existing.UserId;

        var result = await _repo.UpdateAsync(updated);

        await _audit.LogAsync(new AuditLog
        {
            UserId = userId,
            Action = "TaskUpdated",
            EntityType = "Task",
            EntityId = id,
            Metadata = JsonSerializer.Serialize(new
            {
                oldTitle,
                newTitle = updated.Title,
                oldDescription,
                newDescription = updated.Description
            })
        });

        await _notifications.NotifyAsync(new Notification
        {
            UserId = existing.UserId,
            Type = "TaskUpdated",
            Message = $"Your task '{updated.Title}' was updated.",
            EntityType = "Task",
            EntityId = id
        });

        if (!wasComplete && updated.IsComplete)
        {
            await _notifications.NotifyAsync(new Notification
            {
                UserId = existing.UserId,
                Type = "TaskCompleted",
                Message = $"Your task '{updated.Title}' is now complete.",
                EntityType = "Task",
                EntityId = id
            });
        }

        return result;
    }

    // DELETE — enforce ownership + audit + notify
    public async Task<bool> DeleteAsync(int id, int userId, string role)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            throw new DomainException("Task not found.", 404);

        if (existing.UserId != userId && role != "Admin")
            throw new DomainException("You do not have access to this task.", 403);

        var deleted = await _repo.DeleteAsync(id);

        if (deleted)
        {
            await _audit.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "TaskDeleted",
                EntityType = "Task",
                EntityId = id,
                Metadata = JsonSerializer.Serialize(new { existing.Title })
            });

            await _notifications.NotifyAsync(new Notification
            {
                UserId = existing.UserId,
                Type = "TaskDeleted",
                Message = $"Your task '{existing.Title}' was deleted.",
                EntityType = "Task",
                EntityId = id
            });
        }

        return deleted;
    }

    // DELETE ALL — admin only + audit + notify
    public async Task<bool> DeleteAllAsync(int userId, string role)
    {
        if (role != "Admin")
            return false;

        var deleted = await _repo.DeleteAllAsync();

        if (deleted)
        {
            await _audit.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "AllTasksDeleted",
                EntityType = "Task",
                EntityId = null,
                Metadata = null
            });

            await _notifications.NotifyAsync(new Notification
            {
                UserId = userId,
                Type = "AllTasksDeleted",
                Message = "An administrator deleted all tasks.",
                EntityType = "Task",
                EntityId = null
            });
        }

        return deleted;
    }
}