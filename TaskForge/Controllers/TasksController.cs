using Microsoft.AspNetCore.Mvc;
using TaskForge.Api.Models;
using TaskForge.Dtos;
using TaskForge.Services;

namespace TaskForge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    // GET: api/tasks
    [HttpGet]
    public ActionResult<IEnumerable<TaskItem>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    // GET: api/tasks/{id}
    [HttpGet("{id}")]
    public ActionResult<TaskItem> Get(int id)
    {
        var task = _service.Get(id);
        if (task is null)
            return NotFound();

        return Ok(task);
    }

    // POST: api/tasks
    [HttpPost]
    public ActionResult<TaskItem> Create(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description
        };

        var created = _service.Create(task);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id}")]
    public ActionResult<TaskItem> Update(int id, UpdateTaskDto dto)
    {
        var updatedTask = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            IsComplete = dto.IsComplete
        };

        var result = _service.Update(id, updatedTask);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
