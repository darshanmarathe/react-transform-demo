using System.Net.Http;
using System.Net.Http.Json;
using VibeTasks.Wpf.Models;

namespace VibeTasks.Wpf.Services;

public class ApiTaskService
{
    private readonly HttpClient _http;

    public ApiTaskService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TaskItem>> GetAllAsync(bool includeArchived = false)
    {
        using var response = await _http.GetAsync($"api/tasks?includeArchived={includeArchived}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TaskItem>>() ?? new();
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        var payload = new
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

        using var response = await _http.PostAsJsonAsync("api/tasks", payload);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskItem>())!;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        var payload = new
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

        using var response = await _http.PutAsJsonAsync($"api/tasks/{task.Id}", payload);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        using var response = await _http.DeleteAsync($"api/tasks/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ArchiveAsync(int id)
    {
        using var response = await _http.PostAsync($"api/tasks/{id}/archive", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RestoreAsync(int id)
    {
        using var response = await _http.PostAsync($"api/tasks/{id}/restore", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteAsync(int id)
    {
        using var response = await _http.PostAsync($"api/tasks/{id}/complete", null);
        response.EnsureSuccessStatusCode();
    }
}
