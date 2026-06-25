using System.Net.Http;

namespace VibeTasks.Wpf.Services;

public static class ApiConfig
{
    private static readonly Lazy<HttpClient> Client = new(() => new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5000"),
        Timeout = TimeSpan.FromSeconds(30)
    });

    public static HttpClient Http => Client.Value;
    public static ApiTaskService Tasks => new(Http);
    public static ApiUserService Users => new(Http);
    public static ApiExportImportService ExportImport => new(Http);
}
