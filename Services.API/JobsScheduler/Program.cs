using Microsoft.AspNetCore.Builder;
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
    throw; // Hentikan eksekusi jika .env gagal dimuat
}

var builder = WebApplication.CreateBuilder(args);

// Tambahkan DbContext
builder.Services.AddDbContext<BleTrackingDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("BleTrackingDbConnection") ??
                        "Server=192.168.1.116,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));

// Tambahkan logging
builder.Services.AddLogging(logging => logging.AddConsole());

// Tambahkan Quartz
BusinessLogic.Services.Jobs.QuartzConfig.AddQuartzServices(builder.Services);

// Tambahkan controller untuk API
builder.Services.AddControllers();

// Tambahkan endpoint API (opsional untuk debugging Swagger, jika diperlukan)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Konfigurasi pipeline HTTP
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

// Pastikan scheduler Quartz dimulai
var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();
await scheduler.Start();
Console.WriteLine("Quartz Scheduler started at {0:UTC}", DateTime.UtcNow);

await app.RunAsync();