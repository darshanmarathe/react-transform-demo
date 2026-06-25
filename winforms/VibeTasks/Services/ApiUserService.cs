using System.Net.Http.Json;
using VibeTasks.Models;

namespace VibeTasks.Services;

public class ApiUserService
{
    private readonly HttpClient _http;

    public ApiUserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<User>> GetAllAsync()
    {
        var result = await _http.GetAsync("api/users");
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<List<User>>() ?? new();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        var result = await _http.GetAsync($"api/users/{id}");
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<User>();
    }

    public async Task<User> CreateAsync(string name, string email)
    {
        var dto = new { Name = name, Email = email };
        var result = await _http.PostAsJsonAsync("api/users", dto);
        result.EnsureSuccessStatusCode();
        var created = await result.Content.ReadFromJsonAsync<User>();
        return created!;
    }

    public async Task UpdateAsync(User user)
    {
        var dto = new { Name = user.Name, Email = user.Email };
        var result = await _http.PutAsJsonAsync($"api/users/{user.Id}", dto);
        result.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var result = await _http.DeleteAsync($"api/users/{id}");
        result.EnsureSuccessStatusCode();
    }
}
