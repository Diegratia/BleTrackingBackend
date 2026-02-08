using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Repositories.DbContexts;
using BusinessLogic.Services.Implementation.Analytics;
using BusinessLogic.Services.Interface.Analytics;
using Repositories.Repository.Analytics;
using DotNetEnv;
using Data.ViewModels;
using BusinessLogic.Services.Extension.Analytics;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Repositories.Repository;
using Data.ViewModels.Shared.ExceptionHelper;
using BusinessLogic.Services.Extension.RootExtension;
using Microsoft.AspNetCore.Authorization;
using Helpers.Consumer.Mqtt;
using BusinessLogic.Services.Background;

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

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
// === CORS ===
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddAuthorizationNewPolicies();

// === Config ===
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// === DB Context (Read-only) ===
builder.Services.AddDbContext<BleTrackingDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("BleTrackingDbConnection");
    options.UseSqlServer(conn).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// === Authentication ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();


// === Dependencies ===
builder.Services.AddScoped<IAlarmAnalyticsIncidentService, AlarmAnalyticsIncidentService>();
builder.Services.AddScoped<ITrackingSummaryService, TrackingSummaryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITrackingSessionService, TrackingSessionService>();
builder.Services.AddScoped<ITrackingReportPresetService, TrackingReportPresetService>();
builder.Services.AddScoped<IUserJourneyService, UserJourneyService>();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();


builder.Services.AddScoped<TrackingSessionRepository>();
builder.Services.AddScoped<TrackingSummaryRepository>();
builder.Services.AddScoped<AlarmAnalyticsIncidentRepository>();
builder.Services.AddScoped<UserJourneyRepository>();
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<VisitorRepository>();
builder.Services.AddScoped<MstMemberRepository>();
builder.Services.AddScoped<FloorplanDeviceRepository>();
builder.Services.AddScoped<AlarmTriggersRepository>();
builder.Services.AddScoped<FloorplanMaskedAreaRepository>();
builder.Services.AddScoped<TrackingReportPresetRepository>();

builder.Services.AddAutoMapper(typeof(AlarmAnalyticsProfile));
builder.Services.AddAutoMapper(typeof(TrackingAnalyticsProfile));

// === Host config ===
var port = Environment.GetEnvironmentVariable("ANALYTICS_PORT") ??
           builder.Configuration["Ports:AnalyticsService"] ?? "5031";
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var host = env == "Production" ? "0.0.0.0" : "localhost";
builder.WebHost.UseUrls($"http://{host}:{port}");

var app = builder.Build();

// === Middlewares ===
app.MapGet("/hc", async (BleTrackingDbContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
        return Results.Ok(new { code = 200, msg = "Healthy" });
    }
    catch
    {
        return Results.Problem("DB Unreachable", statusCode: 500);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<CustomExceptionMiddleware>(); 
app.UseRouting();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(ResponseCollection<object>.Error(ex.Message));
    }
});

app.MapControllers();
app.Run();

