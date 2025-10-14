using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var app = builder.Build();

app.MapGet("/hc", async (IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory, IConfiguration config) =>
{
    var logger = loggerFactory.CreateLogger("GatewayHealthCheck");
    var client = httpClientFactory.CreateClient();
    var timeout = config.GetValue<int>("HealthCheck:Timeout", 3000);
    var services = config.GetSection("HealthCheck:Services").Get<Dictionary<string, string>>() ?? new();

    var tasks = services.Select(async kvp =>
    {
        var name = kvp.Key;
        var url = kvp.Value;
        var cts = new CancellationTokenSource(timeout);

        try
        {
            var response = await client.GetAsync(url, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            return new
            {
                Name = name,
                Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                Code = (int)response.StatusCode,
                Details = response.IsSuccessStatusCode ? content : null
            };
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Timeout while checking {Service}", name);
            return new { Name = name, Status = "Timeout", Code = 408, Details = (string?)null };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking {Service}", name);
            return new { Name = name, Status = "Unreachable", Code = 500, Details = ex.Message };
        }
    });

    var results = await Task.WhenAll(tasks);
    var allHealthy = results.All(r => r.Status == "Healthy");

    return Results.Json(new
    {
        code = allHealthy ? 200 : 500,
        status = allHealthy ? "Healthy" : "Degraded",
        timestamp = DateTime.UtcNow,
        services = results.ToDictionary(x => x.Name, x => new
        {
            x.Status,
            x.Code,
            x.Details
        })
    }, new JsonSerializerOptions { WriteIndented = true });
});

app.Run("http://0.0.0.0:8080");

