using System.Threading.RateLimiting;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Data.ViewModels.Shared.ExceptionHelper;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository;
using Serilog;

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
builder.Services.AddAutoMapper(typeof(Program).Assembly); // Will find profiles in BusinessLogic.Services
builder.Services.AddAutoMapper(typeof(BusinessLogic.Services.Extension.EvacuationProfile));

builder.Services.AddScoped<IEvacuationAssemblyPointService, EvacuationAssemblyPointService>();
builder.Services.AddScoped<IEvacuationAlertService, EvacuationAlertService>();
builder.Services.AddScoped<IEvacuationTransactionService, EvacuationTransactionService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddSingleton<EvacuationMqttService>();
builder.Services.AddHostedService<MqttRecoveryService>();
builder.Services.AddSingleton<MqttPubQueue>();
builder.Services.AddSingleton<IMqttPubQueue>(sp => sp.GetRequiredService<MqttPubQueue>());
builder.Services.AddHostedService<MqttPubBackgroundService>();

builder.Services.AddScoped<EvacuationAssemblyPointRepository>();
builder.Services.AddScoped<EvacuationAlertRepository>();
builder.Services.AddScoped<EvacuationTransactionRepository>();

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
builder.UseDefaultHostExtension("EVACUATION_PORT", "5040");

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BleTracking Evacuation API");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

// Middleware
app.UseMiddleware<CustomExceptionMiddleware>();

app.UseRouting();
app.UseSerilogRequestLoggingExtension();

app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");

app.Run();
