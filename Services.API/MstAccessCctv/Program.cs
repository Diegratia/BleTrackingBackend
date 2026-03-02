using Shared.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Repositories.DbContexts;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Implementation;
using Microsoft.Extensions.FileProviders;
using BusinessLogic.Services.Interface;
using Repositories.Repository;
using Entities.Models;
using Repositories.Seeding;
using DotNetEnv;
using Data.ViewModels.Shared.ExceptionHelper;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Helpers.Consumer.Mqtt;
using BusinessLogic.Services.Background;

EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddAppsettings();

builder.UseSerilogExtension();
builder.Host.UseWindowsService();
builder.Host.UseSerilog();

builder.Services.AddCorsExtension();
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddValidatorExtensions();
builder.Services.AddSwaggerExtension();

builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();
builder.Services.AddSingleton<MqttPubQueue>();
builder.Services.AddSingleton<IMqttPubQueue>(sp => sp.GetRequiredService<MqttPubQueue>());
builder.Services.AddHostedService<MqttPubBackgroundService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(MstAccessCctvProfile));

// Registrasi Services
builder.Services.AddScoped<IMstAccessCctvService, MstAccessCctvService>();
builder.Services.AddScoped<MstAccessCctvRepository>();

builder.UseDefaultHostExtension("MST_ACCESS_CCTV_PORT", "5005");

var app = builder.Build();

app.UseHealthCheckExtension();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MstAccessCctv API");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseSerilogRequestLoggingExtension();
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();











