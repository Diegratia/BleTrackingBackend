using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Repositories.DbContexts;
using BusinessLogic.Services.Jobs;
using BusinessLogic.Services.Extension.RootExtension;
using Serilog;

// Load .env with proper error handling
EnvTryCatchExtension.LoadEnvWithTryCatch();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/jobscheduler-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting JobsScheduler service...");

    var builder = Host.CreateDefaultBuilder(args);

    // Configure Serilog
    builder.UseSerilog();

    // Configure Windows Service
    builder.UseWindowsService();

    builder.ConfigureServices((hostContext, services) =>
    {
        // Get connection string from environment
        var connectionString = Environment.GetEnvironmentVariable("BleTrackingDbConnection") ??
                                "Server=localhost,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True";

        // Add DbContext
        services.AddDbContext<BleTrackingDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Quartz
        BusinessLogic.Services.Jobs.QuartzConfig.AddQuartzServices(services);
    });

    var host = builder.Build();

    // Ensure Quartz scheduler is started
    var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
    var scheduler = await schedulerFactory.GetScheduler();
    await scheduler.Start();

    Log.Information("Quartz Scheduler started at {Time:UTC}", DateTime.UtcNow);
    Log.Information("JobsScheduler is running as Windows Service");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "JobsScheduler service failed to start");
}
finally
{
    Log.CloseAndFlush();
}
