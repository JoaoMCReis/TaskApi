using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskApi.Data;
using TaskApi.DTO;
using TaskApi.Entities;
using TaskStatus = TaskApi.Entities.TaskStatus;

[ApiController]
[Route("tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetRole() =>
        User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(TaskStatus? status, int page = 1, int pageSize = 10)
    {
        var query = _db.Tasks.AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status);

        var tasks = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Status = TaskStatus.Pending,
            CreatedById = GetUserId(),
            AssignedToId = (Guid)dto.AssignedToId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var role = GetRole();
        var userId = GetUserId();

        // ADMIN e MANAGER podem editar tudo
        if (role == "admin" || role == "manager")
        {
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = (TaskStatus)dto.Status;
            task.AssignedToId = (Guid)dto.AssignedToId;
        }
        // MEMBER só edita o que criou OU onde está atribuído
        else if (role == "member" && (task.CreatedById == userId || task.AssignedToId == userId))
        {
            if (task.CreatedById == userId)
            {
                task.Title = dto.Title;
                task.Description = dto.Description;
                task.Status = (TaskStatus)dto.Status;
                task.AssignedToId = (Guid)dto.AssignedToId;
            }
            else if (task.AssignedToId == userId)
            {
                task.Status = (TaskStatus)dto.Status;
            }
        }
        else
        {
            return Forbid();
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var role = GetRole();
        var userId = GetUserId();

        if (role == "admin" || task.CreatedById == userId)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        return Forbid();
    }
}


