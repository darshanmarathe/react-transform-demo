using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VibeTasks.Api.Data;
using VibeTasks.Api.Models;

namespace VibeTasks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly AppDbContext _db;

    public ImportController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("csv")]
    public async Task<ActionResult<int>> ImportCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var count = 0;

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = ParseCsvLine(line.TrimEnd('\r'));
            if (parts.Length < 11) continue;

            var task = new TaskItem
            {
                Title = parts[1],
                Description = parts[2],
                Status = Enum.Parse<TaskItemStatus>(parts[3]),
                Priority = Enum.Parse<TaskPriority>(parts[4]),
                DueDate = string.IsNullOrEmpty(parts[6])
                    ? null
                    : DateTime.Parse(parts[6], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                IsArchived = bool.Parse(parts[8]),
                IsRecurring = bool.Parse(parts[9]),
                AssignedUserId = string.IsNullOrEmpty(parts[10]) ? null : int.Parse(parts[10])
            };
            _db.Tasks.Add(task);
            count++;
        }

        await _db.SaveChangesAsync();
        return Ok(count);
    }

    [HttpPost("json")]
    public async Task<ActionResult<int>> ImportJson(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        var tasks = JsonSerializer.Deserialize<List<TaskItem>>(content) ?? new();

        foreach (var task in tasks)
        {
            task.Id = 0;
            _db.Tasks.Add(task);
        }

        await _db.SaveChangesAsync();
        return Ok(tasks.Count);
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
