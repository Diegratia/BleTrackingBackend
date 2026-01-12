using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using Repositories.DbContexts;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Implementation;
using Microsoft.Extensions.FileProviders;
using BusinessLogic.Services.Interface;
using Repositories.Repository;
using Entities.Models;
using Repositories.Seeding;
using DotNetEnv;
using Microsoft.Extensions.Hosting;
using BusinessLogic.Services.Extension.RootExtension;
using Data.ViewModels.Shared.ExceptionHelper;
using System.Text.Json.Serialization;
using Serilog;
using Serilog.Events;


try
{
    var possiblePaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),         // lokal root service
        Path.Combine(Directory.GetCurrentDirectory(), "../../.env"),   
        Path.Combine(AppContext.BaseDirectory, ".env"),               
        "/app/.env"                                                 
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


Console.WriteLine("=== SERILOG TEST ===");
Log.Information("Serilog is working - this should go to FILE and CONSOLE");
Console.WriteLine("=== SERILOG TEST END ===");
try
{
    var serviceName = AppDomain.CurrentDomain.FriendlyName
        .Replace(".dll", "")
        .Replace(".exe", "")
        .ToLower();

    bool isDocker = Directory.Exists("/app");
    bool isWindowsService = !(Environment.UserInteractive || System.Diagnostics.Debugger.IsAttached);

    string logDir;

    if (isDocker)
        logDir = "/app/logs";
    else
        logDir = Path.Combine(AppContext.BaseDirectory, $"logs_{serviceName}");

    Directory.CreateDirectory(logDir);

    string logFile = Path.Combine(logDir, $"{serviceName}-log-.txt");


    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", serviceName) 
        .WriteTo.Console(
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Service} | {Message:lj}{NewLine}{Exception}"
        )
        .WriteTo.File(
            logFile,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14, 
            fileSizeLimitBytes: 10 * 1024 * 1024, 
            rollOnFileSizeLimit: true,   
            shared: true,
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Service} | {Message:lj}{NewLine}{Exception}",  
            restrictedToMinimumLevel: LogEventLevel.Information
        )
        .CreateLogger();

    Console.WriteLine($"Serilog initialized → Directory: {logDir}");
}
catch (Exception ex)
{
    Console.WriteLine($"Serilog initialization failed: {ex.Message}");
}



var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Host.UseSerilog();

builder.Services.AddCorsExtension();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;           
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow; 
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddValidatorExtensions();
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddAutoMapper(typeof(PatrolAreaProfile));

builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddSwaggerExtension();

builder.Services.AddScoped<IGeofenceService, GeofenceService>();
builder.Services.AddScoped<IPatrolAreaService, PatrolAreaService>();
builder.Services.AddScoped<IBoundaryService, BoundaryService>();
builder.Services.AddScoped<IStayOnAreaService, StayOnAreaService>();
builder.Services.AddScoped<IOverpopulatingService, OverpopulatingService>();
// builder.Services.AddScoped<IMstIntegrationService, MstIntegrationService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GeofenceRepository>();
builder.Services.AddScoped<OverpopulatingRepository>();
builder.Services.AddScoped<BoundaryRepository>();
builder.Services.AddScoped<StayOnAreaRepository>();
builder.Services.AddScoped<PatrolAreaRepository>();

builder.UseDefaultHostExtension("PATROL_PORT", "5020");
builder.Host.UseWindowsService();


var app = builder.Build();

app.UseHealthCheckExtension();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
    try
    {
        // context.Database.Migrate(); 
        // DatabaseSeeder.Seed(context); 
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during migration or seeding: {ex.Message}");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BleTracking API");
        c.RoutePrefix = "";
    });
}

app.UseCors("AllowAll");
// // app.UseHttpsRedirection();
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseRouting();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "{Service} | {RequestMethod} {RequestPath} | {StatusCode} | {Elapsed:0.0000} ms";

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Service", AppDomain.CurrentDomain.FriendlyName);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);

        var userId = httpContext.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
            diagnosticContext.Set("UserId", userId);
    };
});
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();