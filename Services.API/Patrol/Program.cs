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
using Helpers.Consumer.Mqtt;
using BusinessLogic.Services.Background;


EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);
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
    });

builder.Services.AddValidatorExtensions();  
// builder.Services.AddHostedService<MqttRecoveryService>();                                                                                              
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddAutoMapper(typeof(PatrolAreaProfile));
builder.Services.AddAutoMapper(typeof(PatrolRouteProfile));
builder.Services.AddAutoMapper(typeof(PatrolAssignmentProfile));

builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSwaggerExtension();

builder.Services.AddScoped<IGeofenceService, GeofenceService>();
builder.Services.AddScoped<IPatrolAreaService, PatrolAreaService>();
builder.Services.AddScoped<IPatrolRouteService, PatrolRouteService>();
builder.Services.AddScoped<IPatrolAssignmentService, PatrolAssignmentService>();
builder.Services.AddScoped<IBoundaryService, BoundaryService>();
builder.Services.AddScoped<IStayOnAreaService, StayOnAreaService>();
builder.Services.AddScoped<IOverpopulatingService, OverpopulatingService>();
builder.Services.AddScoped<IPatrolCaseService, PatrolCaseService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();


// builder.Services.AddScoped<IMstIntegrationService, MstIntegrationService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GeofenceRepository>();
builder.Services.AddScoped<OverpopulatingRepository>();
builder.Services.AddScoped<BoundaryRepository>();
builder.Services.AddScoped<StayOnAreaRepository>();
builder.Services.AddScoped<PatrolAreaRepository>();
builder.Services.AddScoped<PatrolRouteRepository>();
builder.Services.AddScoped<PatrolAssignmentRepository>();
builder.Services.AddScoped<TimeGroupRepository>();
builder.Services.AddScoped<PatrolCaseRepository>();
builder.Services.AddScoped<MstSecurityRepository>();

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
app.UseSerilogRequestLoggingExtension();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();