using System.Net.Http.Json;
using VibeTasks.Models;

namespace VibeTasks.Services;

public class ApiExportImportService
{
    private readonly HttpClient _http;

    public ApiExportImportService(HttpClient http)
    {
        _http = http;
    }

    public async Task ExportToCsvAsync(string filePath, bool includeArchived = false)
    {
        var result = await _http.GetAsync($"api/export/csv?includeArchived={includeArchived}");
        result.EnsureSuccessStatusCode();
        var bytes = await result.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(filePath, bytes);
    }

    public async Task ExportToJsonAsync(string filePath, bool includeArchived = false)
    {
        var result = await _http.GetAsync($"api/export/json?includeArchived={includeArchived}");
        result.EnsureSuccessStatusCode();
        var bytes = await result.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(filePath, bytes);
    }

    public async Task<int> ImportFromCsvAsync(string filePath)
    {
        using var form = new MultipartFormDataContent();
        using var stream = File.OpenRead(filePath);
        form.Add(new StreamContent(stream), "file", Path.GetFileName(filePath));
        var result = await _http.PostAsync("api/import/csv", form);
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<int>();
    }

    public async Task<int> ImportFromJsonAsync(string filePath)
    {
        using var form = new MultipartFormDataContent();
        using var stream = File.OpenRead(filePath);
        form.Add(new StreamContent(stream), "file", Path.GetFileName(filePath));
        var result = await _http.PostAsync("api/import/json", form);
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<int>();
    }
}
