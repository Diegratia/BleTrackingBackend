using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Repositories.DbContexts;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Implementation;
using Microsoft.Extensions.FileProviders;
using BusinessLogic.Services.Interface;
using Repositories.Repository;
using Entities.Models;
using Repositories.Seeding;
using DotNetEnv;
using BusinessLogic.Services.Extension.RootExtension;
// using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using BusinessLogic.Services.Background;
using Helpers.Consumer.Mqtt;
using Serilog;
using Serilog.Events;
using Data.ViewModels.Shared.ExceptionHelper;
using BusinessLogic.Services.Extension.FileStorageService;
using Microsoft.AspNetCore.Authorization;
using Helpers.Consumer;
using System.Text.Json.Serialization;



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

builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();


builder.Services.AddRedisExtension(builder.Configuration);
builder.Services.AddHostedService<MqttRecoveryService>();


builder.Services.AddSwaggerExtension();

builder.Services.AddAutoMapper(typeof(MstBuildingProfile));
builder.Services.AddScoped<IMstBuildingService, MstBuildingService>();
builder.Services.AddScoped<IMstFloorService, MstFloorService>();
builder.Services.AddScoped<IMstFloorplanService, MstFloorplanService>();
builder.Services.AddScoped<IFloorplanDeviceService, FloorplanDeviceService>();
builder.Services.AddScoped<IFloorplanMaskedAreaService, FloorplanMaskedAreaService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<MstBuildingRepository>();
builder.Services.AddScoped<MstFloorRepository>();
builder.Services.AddScoped<MstFloorplanRepository>();
builder.Services.AddScoped<FloorplanMaskedAreaRepository>();
builder.Services.AddScoped<FloorplanDeviceRepository>();

builder.Services.AddScoped<IGeofenceService, GeofenceService>();
builder.Services.AddScoped<IBoundaryService, BoundaryService>();
builder.Services.AddScoped<IStayOnAreaService, StayOnAreaService>();
builder.Services.AddScoped<IOverpopulatingService, OverpopulatingService>();
builder.Services.AddScoped<IPatrolAreaService, PatrolAreaService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();


builder.Services.AddScoped<MstAccessCctvRepository>();
builder.Services.AddScoped<MstAccessControlRepository>();
builder.Services.AddScoped<MstBleReaderRepository>();

builder.Services.AddScoped<GeofenceRepository>();
builder.Services.AddScoped<StayOnAreaRepository>();
builder.Services.AddScoped<OverpopulatingRepository>();
builder.Services.AddScoped<BoundaryRepository>();
builder.Services.AddScoped<PatrolAreaRepository>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GeofenceRepository>();
builder.Services.AddScoped<OverpopulatingRepository>();
builder.Services.AddScoped<BoundaryRepository>();
builder.Services.AddScoped<StayOnAreaRepository>();
builder.Services.AddScoped<PatrolAreaRepository>();
builder.Services.AddScoped<PatrolRouteRepository>();
builder.Services.AddScoped<TimeGroupRepository>();

builder.UseDefaultHostExtension("BUILDING_PORT", "5010");
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


var basePath = AppContext.BaseDirectory;
var uploadsPath = Path.Combine(basePath, "Uploads", "BuildingImages");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads/BuildingImages"
});

Directory.CreateDirectory(uploadsPath);

    // var timeoutInSeconds = builder.Configuration.GetValue<int>("RequestTimeout");

    app.UseCors("AllowAll");
    // app.UseHttpsRedirection();
    app.UseRouting();
    app.UseSerilogRequestLoggingExtension();
    app.UseMiddleware<CustomExceptionMiddleware>(); 
    app.UseApiKeyAuthentication();
    app.UseAuthentication();
    app.UseAuthorization(); 
    // app.UseRateLimiter();
    // app.UseRequestTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
    // app.UseFixedWindowRateLimiter(150, TimeSpan.FromMinutes(1));
    app.MapControllers();
    app.Run();














