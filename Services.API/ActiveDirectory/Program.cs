using Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Repositories.DbContexts;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Background;
using Repositories.Repository;
using DotNetEnv;
using Microsoft.Extensions.Hosting;
using BusinessLogic.Services.Extension.RootExtension;
using Data.ViewModels.Shared.ExceptionHelper;
using Microsoft.AspNetCore.Authorization;
using Shared.Contracts;

EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAppsettings();
builder.UseSerilogExtension();

// Konfigurasi CORS
builder.Services.AddCorsExtension();

// Konfigurasi sumber konfigurasi
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Konfigurasi Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
        options.JsonSerializerOptions.Converters.Add(
            new UtcDateTimeConverter()
        );
    });

// Registrasi otomatis validasi FluentValidation
builder.Services.AddValidatorExtensions();

// Konfigurasi DbContext
builder.Services.AddDbContextExtension(builder.Configuration);

// Konfigurasi AutoMapper
builder.Services.AddAutoMapper(typeof(BusinessLogic.Services.Extension.AuthProfile));

// Konfigurasi Autentikasi JWT
builder.Services.AddJwtAuthExtension(builder.Configuration);

// Konfigurasi Otorisasi
builder.Services.AddAuthorizationNewPolicies();

// Konfigurasi Swagger
builder.Services.AddSwaggerExtension();

// Konfigurasi IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrasi Services
// builder.Services.AddScoped<IAdSyncService, AdSyncService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();

// Registrasi Repositories
// builder.Services.AddScoped<ActiveDirectoryConfigRepository>();
builder.Services.AddScoped<MstMemberRepository>();
builder.Services.AddScoped<MstDepartmentRepository>();

// Background Service for scheduled AD sync
// builder.Services.AddHostedService<AdSyncBackgroundService>();

// Konfigurasi port dan host
builder.UseDefaultHostExtension("ACTIVE_DIRECTORY_PORT", "5033");
builder.Host.UseWindowsService();

var app = builder.Build();

app.UseHealthCheckExtension();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BleTracking Active Directory API");
        c.RoutePrefix = "";
    });
}

app.UseCors("AllowAll");
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseRouting();
app.UseSerilogRequestLoggingExtension();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
