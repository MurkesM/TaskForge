using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskForge.Api.Models;
using TaskForge.Dtos;
using TaskForge.Services;

namespace TaskForge.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;
    private readonly IMapper _mapper;

    public TasksController(ITaskService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string CurrentUserRole =>
        User.FindFirst(ClaimTypes.Role)!.Value;

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var tasks = await _service.GetAllAsync(CurrentUserId, CurrentUserRole);
        var dto = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        return Ok(dto);
    }

    // GET: api/tasks/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> Get(int id)
    {
        var task = await _service.GetAsync(id, CurrentUserId, CurrentUserRole);
        if (task is null)
            return NotFound();

        return Ok(_mapper.Map<TaskDto>(task));
    }

    [HttpGet("query")]
    public async Task<IActionResult> Query([FromQuery] TaskQueryParameters query)
    {
        var result = await _service.QueryAsync(query, CurrentUserId, CurrentUserRole);
        return Ok(result);
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        var task = _mapper.Map<TaskItem>(dto);
        var created = await _service.CreateAsync(task, CurrentUserId);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<TaskDto>(created));
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> Update(int id, UpdateTaskDto dto)
    {
        var updatedEntity = _mapper.Map<TaskItem>(dto);

        var updated = await _service.UpdateAsync(id, updatedEntity, CurrentUserId, CurrentUserRole);
        if (updated is null)
            return NotFound();

        return Ok(_mapper.Map<TaskDto>(updated));
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id, CurrentUserId, CurrentUserRole);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/tasks (admin only)
    [HttpDelete]
    public async Task<ActionResult> DeleteAll()
    {
        var success = await _service.DeleteAllAsync(CurrentUserId, CurrentUserRole);
        if (!success)
            return Forbid();

        return NoContent();
    }
}