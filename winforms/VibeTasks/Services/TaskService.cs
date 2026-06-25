using Microsoft.EntityFrameworkCore;
using VibeTasks.Data;
using VibeTasks.Models;

namespace VibeTasks.Services;

public class TaskService
{
    public async Task<List<TaskItem>> GetAllAsync(bool includeArchived = false)
    {
        using var db = new AppDbContext();
        var query = db.Tasks.Include(t => t.AssignedUser).AsQueryable();
        if (!includeArchived)
            query = query.Where(t => !t.IsArchived);
        return await query.OrderByDescending(t => t.Priority)
                          .ThenBy(t => t.Status)
                          .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        using var db = new AppDbContext();
        return await db.Tasks.Include(t => t.AssignedUser)
                             .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        using var db = new AppDbContext();
        task.CreatedAt = DateTime.UtcNow;
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        using var db = new AppDbContext();
        db.Tasks.Update(task);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var db = new AppDbContext();
        var task = await db.Tasks.FindAsync(id);
        if (task != null)
        {
            db.Tasks.Remove(task);
            await db.SaveChangesAsync();
        }
    }

    public async Task ArchiveAsync(int id)
    {
        using var db = new AppDbContext();
        var task = await db.Tasks.FindAsync(id);
        if (task != null)
        {
            task.IsArchived = true;
            await db.SaveChangesAsync();
        }
    }

    public async Task RestoreAsync(int id)
    {
        using var db = new AppDbContext();
        var task = await db.Tasks.FindAsync(id);
        if (task != null)
        {
            task.IsArchived = false;
            await db.SaveChangesAsync();
        }
    }

    public async Task CompleteAsync(int id)
    {
        using var db = new AppDbContext();
        var task = await db.Tasks.FindAsync(id);
        if (task != null)
        {
            task.Status = TaskItemStatus.Done;
            task.CompletedAt = DateTime.UtcNow;

            if (task.IsRecurring && task.RecurrenceInterval.HasValue)
            {
                task.DueDate = task.RecurrenceInterval.Value switch
                {
                    RecurrenceInterval.Daily => DateTime.UtcNow.AddDays(1),
                    RecurrenceInterval.Weekly => DateTime.UtcNow.AddDays(7),
                    RecurrenceInterval.Monthly => DateTime.UtcNow.AddMonths(1),
                    RecurrenceInterval.Yearly => DateTime.UtcNow.AddYears(1),
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

            await db.SaveChangesAsync();
        }
    }
}
