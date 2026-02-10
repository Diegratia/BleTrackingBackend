using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
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
using Serilog;
using Serilog.Events;

EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);
builder.UseSerilogExtension();
builder.Host.UseWindowsService();
builder.Host.UseSerilog();

// === CORS ===
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddAuthorizationNewPolicies();

// === Config ===
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRedisExtension(builder.Configuration);
builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddSwaggerExtension();

// === Dependencies ===
builder.Services.AddScoped<IAlarmAnalyticsIncidentService, AlarmAnalyticsIncidentService>();
builder.Services.AddScoped<ITrackingSummaryService, TrackingSummaryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITrackingSessionService, TrackingSessionService>();
builder.Services.AddScoped<ITrackingReportPresetService, TrackingReportPresetService>();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();
builder.Services.AddSingleton<MqttPubQueue>();
builder.Services.AddSingleton<IMqttPubQueue>(sp => sp.GetRequiredService<MqttPubQueue>());
builder.Services.AddHostedService<MqttPubBackgroundService>();

builder.Services.AddScoped<TrackingSessionRepository>();
builder.Services.AddScoped<TrackingSummaryRepository>();
builder.Services.AddScoped<AlarmAnalyticsIncidentRepository>();
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<VisitorRepository>();
builder.Services.AddScoped<MstMemberRepository>();
builder.Services.AddScoped<FloorplanDeviceRepository>();
builder.Services.AddScoped<AlarmTriggersRepository>();
builder.Services.AddScoped<FloorplanMaskedAreaRepository>();
builder.Services.AddScoped<TrackingReportPresetRepository>();

builder.Services.AddAutoMapper(typeof(AlarmAnalyticsProfile));
builder.Services.AddAutoMapper(typeof(TrackingAnalyticsProfile));

// === DB Context (Read-only) ===
builder.Services.AddDbContext<BleTrackingDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("BleTrackingDbConnection");
    options.UseSqlServer(conn).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

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
app.UseSerilogRequestLoggingExtension();
app.UseRouting();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
