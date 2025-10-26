using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Repositories.DbContexts;
using BusinessLogic.Services.Jobs;
using DotNetEnv;

try
{
    Env.Load("/app/.env");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load .env file: {ex.Message}");
}

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddDbContext<BleTrackingDbContext>(options =>
        options.UseSqlServer(hostContext.Configuration.GetConnectionString("BleTrackingDbConnection") ??
                            "Server=192.168.1.116,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));
    services.AddLogging(logging => logging.AddConsole());
    QuartzConfig.AddQuartzServices(services);
});
var host = builder.Build();
// Pastikan scheduler dimulai
var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();
await scheduler.Start();

// Jalankan host
await host.RunAsync();