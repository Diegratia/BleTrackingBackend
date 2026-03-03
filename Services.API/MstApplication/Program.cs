using Shared.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using Repositories.DbContexts;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Implementation.Analytics;
using Microsoft.Extensions.FileProviders;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Interface.Analytics;
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
using Helpers.Consumer.Mqtt;
using BusinessLogic.Services.Background;
using Microsoft.AspNetCore.Authorization;
using BusinessLogic.Services.Extension.FileStorageService;
using BusinessLogic.Services.Extension.Encrypt;
using Repositories.Repository.Analytics;


EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddAppsettings();
builder.UseSerilogExtension();   
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

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
        options.JsonSerializerOptions.Converters.Add(
            new UtcDateTimeConverter()
        );
    });

builder.Services.AddValidatorExtensions();  
// builder.Services.AddHostedService<MqttRecoveryService>();                                                                                              
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddRedisExtension(builder.Configuration);

builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSwaggerExtension();

builder.Services.AddAutoMapper(typeof(MstApplicationProfile));
// Registrasi Services
builder.Services.AddScoped<IMstApplicationService, MstApplicationService>();
builder.Services.AddScoped<IMstIntegrationService, MstIntegrationService>();
builder.Services.AddScoped<IMstBrandService, MstBrandService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();
builder.Services.AddSingleton<MqttPubQueue>();
builder.Services.AddSingleton<IMqttPubQueue>(sp => sp.GetRequiredService<MqttPubQueue>());
builder.Services.AddHostedService<MqttPubBackgroundService>();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();


builder.Services.AddScoped<MstApplicationRepository>();
builder.Services.AddScoped<UserGroupRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<MstBrandRepository>();
builder.Services.AddScoped<MstIntegrationRepository>();
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<GroupBuildingAccessRepository>();


builder.Services.AddScoped<IEmailService, EmailService>();

// builder.Services.AddScoped<IMstIntegrationService, MstIntegrationService>();


builder.Services.AddHttpContextAccessor();


builder.UseDefaultHostExtension("MST_APPLICATION_PORT", "5007");
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
app.UseSerilogRequestLoggingExtension();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();














