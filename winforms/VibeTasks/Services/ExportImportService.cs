using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VibeTasks.Data;
using VibeTasks.Models;

namespace VibeTasks.Services;

public class ExportImportService
{
    private readonly TaskService _taskService = new();

    public async Task ExportToCsvAsync(string filePath, bool includeArchived = false)
    {
        var tasks = await _taskService.GetAllAsync(includeArchived);
        var lines = new List<string>
        {
            "Id,Title,Description,Status,Priority,CreatedAt,DueDate,CompletedAt,IsArchived,IsRecurring,AssignedUserId"
        };

        foreach (var t in tasks)
        {
            lines.Add(string.Join(",",
                t.Id,
                CsvEscape(t.Title),
                CsvEscape(t.Description),
                t.Status,
                t.Priority,
                t.CreatedAt.ToString("O"),
                t.DueDate?.ToString("O") ?? "",
                t.CompletedAt?.ToString("O") ?? "",
                t.IsArchived,
                t.IsRecurring,
                t.AssignedUserId?.ToString() ?? ""));
        }

        await File.WriteAllLinesAsync(filePath, lines);
    }

    public async Task ExportToJsonAsync(string filePath, bool includeArchived = false)
    {
        var tasks = await _taskService.GetAllAsync(includeArchived);
        var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<List<TaskItem>> ImportFromCsvAsync(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        var tasks = new List<TaskItem>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = ParseCsvLine(line);
            if (parts.Length < 11) continue;

            tasks.Add(new TaskItem
            {
                Title = parts[1],
                Description = parts[2],
                Status = Enum.Parse<TaskItemStatus>(parts[3]),
                Priority = Enum.Parse<TaskPriority>(parts[4]),
                DueDate = string.IsNullOrEmpty(parts[6]) ? null : DateTime.Parse(parts[6], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                IsArchived = bool.Parse(parts[8]),
                IsRecurring = bool.Parse(parts[9]),
                AssignedUserId = string.IsNullOrEmpty(parts[10]) ? null : int.Parse(parts[10])
            });
        }

        foreach (var task in tasks)
            await _taskService.CreateAsync(task);

        return tasks;
    }

    public async Task<List<TaskItem>> ImportFromJsonAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();
        foreach (var task in tasks)
        {
            task.Id = 0;
            await _taskService.CreateAsync(task);
        }
        return tasks;
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
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
