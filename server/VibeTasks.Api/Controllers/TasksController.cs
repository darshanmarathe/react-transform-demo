using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibeTasks.Api.Data;
using VibeTasks.Api.Models;

namespace VibeTasks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskItem>>> GetAll([FromQuery] bool includeArchived = false)
    {
        var query = _db.Tasks.Include(t => t.AssignedUser).AsQueryable();
        if (!includeArchived)
            query = query.Where(t => !t.IsArchived);
        var tasks = await query.OrderByDescending(t => t.Priority)
                               .ThenBy(t => t.Status)
                               .ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _db.Tasks.Include(t => t.AssignedUser)
                                  .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(TaskCreateDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            IsRecurring = dto.IsRecurring,
            RecurrenceInterval = dto.RecurrenceInterval,
            RecurrenceCount = dto.RecurrenceCount,
            AssignedUserId = dto.AssignedUserId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaskUpdateDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.Status.HasValue) task.Status = dto.Status.Value;
        if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
        if (dto.DueDate != null) task.DueDate = dto.DueDate;
        if (dto.IsArchived.HasValue) task.IsArchived = dto.IsArchived.Value;
        if (dto.IsRecurring.HasValue) task.IsRecurring = dto.IsRecurring.Value;
        if (dto.RecurrenceInterval != null) task.RecurrenceInterval = dto.RecurrenceInterval;
        if (dto.RecurrenceCount != null) task.RecurrenceCount = dto.RecurrenceCount;
        if (dto.AssignedUserId != null) task.AssignedUserId = dto.AssignedUserId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        task.IsArchived = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        task.IsArchived = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.Status = TaskItemStatus.Done;
        task.CompletedAt = DateTime.UtcNow;

        if (task.IsRecurring && task.RecurrenceInterval.HasValue)
        {
            task.DueDate = task.RecurrenceInterval.Value switch
            {
                Models.RecurrenceInterval.Daily => DateTime.UtcNow.AddDays(1),
                Models.RecurrenceInterval.Weekly => DateTime.UtcNow.AddDays(7),
                Models.RecurrenceInterval.Monthly => DateTime.UtcNow.AddMonths(1),
                Models.RecurrenceInterval.Yearly => DateTime.UtcNow.AddYears(1),
                _ => null
            };

            task.Status = TaskItemStatus.Todo;
            task.CompletedAt = null;

            if (task.RecurrenceCount.HasValue)
            {
                task.RecurrenceCount--;
                if (task.RecurrenceCount <= 0)
                    task.IsRecurring = false;
            }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
