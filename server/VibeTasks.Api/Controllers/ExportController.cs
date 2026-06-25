using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibeTasks.Api.Data;

namespace VibeTasks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _db;

    public ExportController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] bool includeArchived = false)
    {
        var query = _db.Tasks.Include(t => t.AssignedUser).AsQueryable();
        if (!includeArchived)
            query = query.Where(t => !t.IsArchived);
        var tasks = await query.OrderByDescending(t => t.Priority).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,Title,Description,Status,Priority,CreatedAt,DueDate,CompletedAt,IsArchived,IsRecurring,AssignedUserId");
        foreach (var t in tasks)
        {
            sb.AppendLine(string.Join(",",
                t.Id,
                CsvEscape(t.Title),
                CsvEscape(t.Description),
                t.Status, t.Priority,
                t.CreatedAt.ToString("O"),
                t.DueDate?.ToString("O") ?? "",
                t.CompletedAt?.ToString("O") ?? "",
                t.IsArchived, t.IsRecurring,
                t.AssignedUserId?.ToString() ?? ""));
        }

        return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "vibetasks_export.csv");
    }

    [HttpGet("json")]
    public async Task<IActionResult> ExportJson([FromQuery] bool includeArchived = false)
    {
        var query = _db.Tasks.Include(t => t.AssignedUser).AsQueryable();
        if (!includeArchived)
            query = query.Where(t => !t.IsArchived);
        var tasks = await query.OrderByDescending(t => t.Priority).ToListAsync();

        var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });

        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "vibetasks_export.json");
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
