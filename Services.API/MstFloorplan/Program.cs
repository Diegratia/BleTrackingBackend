using BusinessLogic.Services.Background;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Extension.FileStorageService;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Data.ViewModels.Shared.ExceptionHelper;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Repositories.DbContexts;
using Repositories.Repository;
using Serilog;
using System.Threading.RateLimiting;

// 1. Load Env
EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);

// 2. Setup Serilog & Host
builder.UseSerilogExtension();
builder.Host.UseWindowsService();
builder.Host.UseSerilog();

// 3. Setup Services
builder.Services.AddCorsExtension();
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddRedisExtension(builder.Configuration);

// 4. Setup Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddValidatorExtensions();
builder.Services.AddMemoryCache();

// 5. Auth & Swagger
builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSwaggerExtension();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();

// 6. Application Services & Repos
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(MstFloorplanProfile));

builder.Services.AddScoped<IMstFloorplanService, MstFloorplanService>();
builder.Services.AddScoped<IFloorplanDeviceService, FloorplanDeviceService>();
builder.Services.AddScoped<IFloorplanMaskedAreaService, FloorplanMaskedAreaService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();

builder.Services.AddScoped<MstFloorplanRepository>();
builder.Services.AddScoped<FloorplanDeviceRepository>();
builder.Services.AddScoped<FloorplanMaskedAreaRepository>();
builder.Services.AddScoped<MstAccessCctvRepository>();
builder.Services.AddScoped<MstAccessControlRepository>();
builder.Services.AddScoped<MstBleReaderRepository>();
builder.Services.AddScoped<GeofenceRepository>();
builder.Services.AddScoped<BoundaryRepository>();
builder.Services.AddScoped<StayOnAreaRepository>();
builder.Services.AddScoped<OverpopulatingRepository>();
builder.Services.AddScoped<PatrolAreaRepository>();

builder.Services.AddScoped<IGeofenceService, GeofenceService>();
builder.Services.AddScoped<IBoundaryService, BoundaryService>();
builder.Services.AddScoped<IStayOnAreaService, StayOnAreaService>();
builder.Services.AddScoped<IOverpopulatingService, OverpopulatingService>();
builder.Services.AddScoped<IPatrolAreaService, PatrolAreaService>();

// 7. Rate Limit
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 150;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// 8. Port config
builder.UseDefaultHostExtension("MST_FLOORPLAN_PORT", "5014");

var app = builder.Build();

// 9. Pipeline
app.UseHealthCheckExtension();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
    // context.Database.Migrate();
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

var basePath = AppContext.BaseDirectory;
var uploadsPath = Path.Combine(basePath, "Uploads", "FloorplanImages");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads/FloorplanImages"
});

// Middleware
app.UseMiddleware<CustomExceptionMiddleware>();

app.UseRouting();
app.UseSerilogRequestLoggingExtension();

app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");

app.Run();
