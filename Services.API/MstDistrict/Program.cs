using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Repositories.DbContexts;
using Repositories.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DotNetEnv;
using Serilog;
using Repositories.Seeding;
using Data.ViewModels.Shared.ExceptionHelper;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Helpers.Consumer.Mqtt;
using BusinessLogic.Services.Background; // For StatusCodes

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
builder.Services.AddAutoMapper(typeof(MstDistrictProfile));

builder.Services.AddScoped<IMstDistrictService, MstDistrictService>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();
builder.Services.AddScoped<MstDistrictRepository>();


// 7. Rate Limit
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 150;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});


// 8. Port config
builder.UseDefaultHostExtension("MST_DISTRICT_PORT", "5012");


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

// Middleware
app.UseMiddleware<CustomExceptionMiddleware>();

app.UseRouting();
app.UseSerilogRequestLoggingExtension();

app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed"); 

app.Run();
