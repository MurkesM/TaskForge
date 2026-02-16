using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using TaskForge.Api.Models;
using TaskForge.Dtos;
using TaskForge.Services;

namespace TaskForge.Controllers;

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

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var tasks = await _service.GetAllAsync();
        var dto = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        return Ok(dto);
    }

    // GET: api/tasks/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> Get(int id)
    {
        var task = await _service.GetAsync(id);
        if (task is null)
            return NotFound();

        var dto = _mapper.Map<TaskDto>(task);
        return Ok(dto);
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        var task = _mapper.Map<TaskItem>(dto);
        var created = await _service.CreateAsync(task);

        var resultDto = _mapper.Map<TaskDto>(created);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, resultDto);
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> Update(int id, UpdateTaskDto dto)
    {
        var updatedEntity = _mapper.Map<TaskItem>(dto);

        var updated = await _service.UpdateAsync(id, updatedEntity);
        if (updated is null)
            return NotFound();

        var resultDto = _mapper.Map<TaskDto>(updated);
        return Ok(resultDto);
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}