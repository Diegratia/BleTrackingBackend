using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Repositories.DbContexts;
using BusinessLogic.Services.Jobs;
using DotNetEnv;

try
{
    Env.Load("/app/.env");
    Console.WriteLine("Successfully loaded .env file");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load .env file: {ex.Message}");
    throw;
}

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    // Tambahkan logging
    services.AddLogging(logging => logging.AddConsole());

    // Tambahkan DbContext
    services.AddDbContext<BleTrackingDbContext>(options =>
        options.UseSqlServer(Environment.GetEnvironmentVariable("BleTrackingDbConnection") ??
                            "Server=192.168.1.116,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));

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