using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Repositories.DbContexts;
using BusinessLogic.Services.Jobs;
using DotNetEnv;

try
{
    var possiblePaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),         // lokal root service
        Path.Combine(Directory.GetCurrentDirectory(), "../../.env"),   // lokal di subfolder Services.API
        Path.Combine(AppContext.BaseDirectory, ".env"),                // hasil publish
        "/app/.env"                                                   // path dalam Docker container
    };

    var envFile = possiblePaths.FirstOrDefault(File.Exists);

    if (envFile != null)
    {
        Console.WriteLine($"Loading env file: {envFile}");
        Env.Load(envFile);
    }
    else
    {
        Console.WriteLine("No .env file found — skipping load");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load .env file: {ex.Message}");
}


var builder = Host.CreateDefaultBuilder(args);
// builder.Host.UseWindowsService();

builder.ConfigureServices((hostContext, services) =>
{
    // Tambahkan logging
    services.AddLogging(logging => logging.AddConsole());

    // Tambahkan DbContext
    services.AddDbContext<BleTrackingDbContext>(options =>
        options.UseSqlServer(Environment.GetEnvironmentVariable("BleTrackingDbConnection") ??
                            "Server=localhost,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));

    // Tambahkan Quartz
    BusinessLogic.Services.Jobs.QuartzConfig.AddQuartzServices(services);
});

var host = builder.Build();

// Pastikan scheduler Quartz dimulai
var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();
await scheduler.Start();
host.Services.GetRequiredService<ILogger<Program>>().LogInformation("Quartz Scheduler started at {Time:UTC}", DateTime.UtcNow);

await host.RunAsync();