using System.IO;
using System.Net.Http;
using System.Net.Http.Json;

namespace VibeTasks.Wpf.Services;

public class ApiExportImportService
{
    private readonly HttpClient _http;

    public ApiExportImportService(HttpClient http)
    {
        _http = http;
    }

    public async Task ExportToCsvAsync(string filePath, bool includeArchived)
    {
        using var response = await _http.GetAsync($"api/export/csv?includeArchived={includeArchived}");
        response.EnsureSuccessStatusCode();
        await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
    }

    public async Task ExportToJsonAsync(string filePath, bool includeArchived)
    {
        using var response = await _http.GetAsync($"api/export/json?includeArchived={includeArchived}");
        response.EnsureSuccessStatusCode();
        await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
    }

    public async Task<int> ImportFromCsvAsync(string filePath)
    {
        return await UploadAsync("api/import/csv", filePath);
    }

    public async Task<int> ImportFromJsonAsync(string filePath)
    {
        return await UploadAsync("api/import/json", filePath);
    }

    private async Task<int> UploadAsync(string route, string filePath)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = File.OpenRead(filePath);
        content.Add(new StreamContent(stream), "file", Path.GetFileName(filePath));
        using var response = await _http.PostAsync(route, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }
}
