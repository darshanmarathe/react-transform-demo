using System.Net.Http.Json;
using VibeTasks.Models;

namespace VibeTasks.Services;

public class ApiTaskService
{
    private readonly HttpClient _http;

    public ApiTaskService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TaskItem>> GetAllAsync(bool includeArchived = false)
    {
        var result = await _http.GetAsync($"api/tasks?includeArchived={includeArchived}");
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<List<TaskItem>>() ?? new();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        var result = await _http.GetAsync($"api/tasks/{id}");
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<TaskItem>();
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        var dto = new
        {
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.IsRecurring,
            task.RecurrenceInterval,
            task.RecurrenceCount,
            task.AssignedUserId
        };
        var result = await _http.PostAsJsonAsync("api/tasks", dto);
        result.EnsureSuccessStatusCode();
        var created = await result.Content.ReadFromJsonAsync<TaskItem>();
        return created!;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        var dto = new
        {
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.IsArchived,
            task.IsRecurring,
            task.RecurrenceInterval,
            task.RecurrenceCount,
            task.AssignedUserId
        };
        var result = await _http.PutAsJsonAsync($"api/tasks/{task.Id}", dto);
        result.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var result = await _http.DeleteAsync($"api/tasks/{id}");
        result.EnsureSuccessStatusCode();
    }

    public async Task ArchiveAsync(int id)
    {
        var result = await _http.PostAsync($"api/tasks/{id}/archive", null);
        result.EnsureSuccessStatusCode();
    }

    public async Task RestoreAsync(int id)
    {
        var result = await _http.PostAsync($"api/tasks/{id}/restore", null);
        result.EnsureSuccessStatusCode();
    }

    public async Task CompleteAsync(int id)
    {
        var result = await _http.PostAsync($"api/tasks/{id}/complete", null);
        result.EnsureSuccessStatusCode();
    }
}
