using System.Net.Http;
using System.Net.Http.Json;
using VibeTasks.Wpf.Models;

namespace VibeTasks.Wpf.Services;

public class ApiUserService
{
    private readonly HttpClient _http;

    public ApiUserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<User>> GetAllAsync()
    {
        using var response = await _http.GetAsync("api/users");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<User>>() ?? new();
    }

    public async Task<User> CreateAsync(string name, string email)
    {
        using var response = await _http.PostAsJsonAsync("api/users", new { Name = name, Email = email });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<User>())!;
    }

    public async Task UpdateAsync(User user)
    {
        using var response = await _http.PutAsJsonAsync($"api/users/{user.Id}", new { user.Name, user.Email });
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        using var response = await _http.DeleteAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
    }
}
