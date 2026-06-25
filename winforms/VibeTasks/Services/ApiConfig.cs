namespace VibeTasks.Services;

public static class ApiConfig
{
    private static readonly Lazy<HttpClient> _http = new(() =>
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        return client;
    });

    public static HttpClient Http => _http.Value;

    public static ApiTaskService Tasks => new(Http);
    public static ApiUserService Users => new(Http);
    public static ApiExportImportService ExportImport => new(Http);
}
