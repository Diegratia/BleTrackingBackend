using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Repositories.DbContexts;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Repositories.Repository;
using Repositories.Repository.RepoModel;
using FluentValidation;
using FluentValidation.AspNetCore;
using Helpers.Consumer.Mqtt;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Extension.RootExtension;
using Microsoft.AspNetCore.Authorization;
using Data.ViewModels.Shared.ExceptionHelper;

EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Services.AddHostedService<MqttRecoveryService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Registrasi otomatis validasi FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Scan semua validator di assembly yang mengandung BrandValidator
builder.Services.AddValidatorsFromAssemblyContaining<CardCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CardUpdateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SwapCreateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SwapForwardValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SwapReverseValidator>();

builder.UseSerilogExtension();
builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddSwaggerExtension();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(CardProfile), typeof(CardSwapTransactionProfile));

// Registrasi Services
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICardSwapTransactionService, CardSwapTransactionService>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();

// Registrasi Repositories
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<CardAccessRepository>();
builder.Services.AddScoped<MstMemberRepository>();
builder.Services.AddScoped<CardSwapTransactionRepository>();

var port = Environment.GetEnvironmentVariable("CARD_PORT") ??
           builder.Configuration["Ports:CardService"] ?? "5026";
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var host = env == "Production" ? "0.0.0.0" : "localhost";
builder.WebHost.UseUrls($"http://{host}:{port}");

var app = builder.Build();

app.UseSerilogRequestLoggingExtension();

app.MapGet("/hc", async (IServiceProvider sp) =>
{
    var db = sp.GetRequiredService<BleTrackingDbContext>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HealthCheck");

    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1"); // cek koneksi DB
        return Results.Ok(new
        {
            code = 200,
            msg = "Healthy",
            details = new
            {
                database = "Connected"
            }
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Health check failed");
        return Results.Problem("Database unreachable", statusCode: 500);
    }
})
.AllowAnonymous();

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
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseRouting();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


