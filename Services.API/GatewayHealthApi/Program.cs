using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();

builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var app = builder.Build();

// ====================================
// WARMUP LANGSUNG DI SINI (NO CLASS)
// ====================================
app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        var sp = app.Services;
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var config = sp.GetRequiredService<IConfiguration>();
        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient();

        try
        {
            // === Tambahkan token dan API key ke header ===
            var jwtToken = config.GetValue<string>("HealthCheck:WarmupToken");
            var apiKey = config.GetValue<string>("HealthCheck:X-BIOPEOPLETRACKING-API-KEY");

            if (!string.IsNullOrWhiteSpace(jwtToken))
                client.DefaultRequestHeaders.Add("Authorization", jwtToken.Trim());

            if (!string.IsNullOrWhiteSpace(apiKey))
                client.DefaultRequestHeaders.Add("X-BIOPEOPLETRACKING-API-KEY", apiKey.Trim());

            // === 1️⃣ HEALTH WARMUP ===
            var healthUrls = config.GetSection("HealthCheck:Services").Get<Dictionary<string, string>>() ?? new();
            logger.LogInformation("Starting health warmup for {Count} services...", healthUrls.Count);

            var healthTasks = healthUrls.Select(async kvp =>
            {
                try
                {
                    var response = await client.GetAsync(kvp.Value);
                    logger.LogInformation("HC {Name} ({Url}) -> {Status}", kvp.Key, kvp.Value, (int)response.StatusCode);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "HC failed: {Name} ({Url})", kvp.Key, kvp.Value);
                }
            });

            await Task.WhenAll(healthTasks);
            logger.LogInformation("Health warmup completed!");

            // === 2️⃣ CACHE WARMUP (GetAll) ===
            var warmupEndpoints = config.GetSection("HealthCheck:WarmupEndpoints").Get<Dictionary<string, string>>() ?? new();
            logger.LogInformation("Starting cache warmup for {Count} endpoints...", warmupEndpoints.Count);

            var warmupTasks = warmupEndpoints.Select(async kvp =>
            {
                try
                {
                    var response = await client.GetAsync(kvp.Value);
                    logger.LogInformation("Cache warmup {Name} ({Url}) -> {Status}", kvp.Key, kvp.Value, (int)response.StatusCode);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Cache warmup failed: {Name} ({Url})", kvp.Key, kvp.Value);
                }
            });

            await Task.WhenAll(warmupTasks);
            logger.LogInformation("✅ Cache warmup completed!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "🔥 Warmup process failed critically");
        }
    });
});


// kalau pakai ini bisa jadi sebelum api redi sudah warmup
// _ = Task.Run(async () =>
// {
//     try
//     {
//         var client = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
//         var config = app.Services.GetRequiredService<IConfiguration>();
//         var logger = app.Services.GetRequiredService<ILogger<Program>>();

//         var services = config.GetSection("HealthCheck:Services")
//                             .Get<Dictionary<string, string>>() ?? new();

//         logger.LogInformation("Starting warmup for {Count} services...", services.Count);

//         var tasks = services.Select(async kvp =>
//         {
//             try
//             {
//                 var response = await client.GetAsync(kvp.Value);
//                 logger.LogInformation("Warmup {Name} ({Url}) -> {Status}", kvp.Key, kvp.Value, (int)response.StatusCode);
//             }
//             catch (Exception ex)
//             {
//                 logger.LogWarning(ex, "Warmup failed: {Name} ({Url})", kvp.Key, kvp.Value);
//             }
//         });

//         await Task.WhenAll(tasks);
//         logger.LogInformation("Warmup completed!");
//     }
//     catch (Exception ex)
//     {
//         app.Services.GetRequiredService<ILogger<Program>>().LogError(ex, "Warmup failed critically");
//     }
// });
// ====================================

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
            return new
            {
                Name = name,
                Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                Code = (int)response.StatusCode
            };
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Timeout while checking {Service}", name);
            return new { Name = name, Status = "Timeout", Code = 408 };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking {Service}", name);
            return new { Name = name, Status = "Unreachable", Code = 500 };
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
            x.Code
        })
    }, new JsonSerializerOptions { WriteIndented = true });
});

app.Run("http://0.0.0.0:8080");