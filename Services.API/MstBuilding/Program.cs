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


try
{
    var possiblePaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),         // lokal root service
        Path.Combine(Directory.GetCurrentDirectory(), "../../.env"),   // lokal di subfolder Services.API
        Path.Combine(AppContext.BaseDirectory, ".env"),                // hasil publish
        "/app/.env"                                                   // path dalam Docker container
    };

    var envFile = possiblePaths.FirstOrDefault(File.Exists);

    if (envFile != null)
    {
        Console.WriteLine($"Loading env file: {envFile}");
        Env.Load(envFile);
    }
    else
    {
        Console.WriteLine("No .env file found — skipping load");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load .env file: {ex.Message}");
}


// =============================
//  SERILOG UNIVERSAL SETUP
// =============================
Console.WriteLine("=== SERILOG TEST ===");
Log.Information("Serilog is working - this should go to FILE and CONSOLE");
Console.WriteLine("=== SERILOG TEST END ===");
try
{
    var serviceName = AppDomain.CurrentDomain.FriendlyName
        .Replace(".dll", "")
        .Replace(".exe", "")
        .ToLower();

    bool isDocker = Directory.Exists("/app");
    bool isWindowsService = !(Environment.UserInteractive || System.Diagnostics.Debugger.IsAttached);

    string logDir;

    if (isDocker)
        logDir = "/app/logs";
    else
        logDir = Path.Combine(AppContext.BaseDirectory, $"logs_{serviceName}");

    Directory.CreateDirectory(logDir);

    string logFile = Path.Combine(logDir, $"{serviceName}-log-.txt");


    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        // .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", serviceName) 
        .WriteTo.Console(
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Service} | {Message:lj}{NewLine}{Exception}"
        )
        .WriteTo.File(
            logFile,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14, // keep 14 hari
            fileSizeLimitBytes: 10 * 1024 * 1024, 
            rollOnFileSizeLimit: true,   
            shared: true,
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Service} | {Message:lj}{NewLine}{Exception}",  // ✅ SAMA dengan console
            restrictedToMinimumLevel: LogEventLevel.Information
        )
        .CreateLogger();

    Console.WriteLine($"Serilog initialized → Directory: {logDir}");
}
catch (Exception ex)
{
    Console.WriteLine($"Serilog initialization failed: {ex.Message}");
}



var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Host.UseSerilog();

builder.Services.AddMemoryCache();
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

builder.Services.AddControllers();

builder.Services.AddDbContext<BleTrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BleTrackingDbConnection")));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
    options.AddPolicy("RequiredSystemUser", policy =>
        policy.RequireRole("System"));
    options.AddPolicy("RequirePrimaryRole", policy =>
        policy.RequireRole("Primary"));
    options.AddPolicy("RequireSuperAdminRole", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("RequireSystemOrSuperAdminRole", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin"));
    });

    options.AddPolicy("RequirePrimaryOrSystemOrPrimaryAdminRole", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("Primary"));
    });
    options.AddPolicy("RequirePrimaryAdminOrSystemOrSuperAdminRole", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("PrimaryAdmin"));
    });
    options.AddPolicy("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole", policy =>
{
    policy.RequireAssertion(context =>
        context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("PrimaryAdmin") || context.User.IsInRole("Secondary"));
});
    options.AddPolicy("RequireAll", policy =>
     {
         policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("PrimaryAdmin") || context.User.IsInRole("Primary") || context.User.IsInRole("Secondary"));
     });
    options.AddPolicy("RequireUserCreatedRole", policy =>
        policy.RequireRole("UserCreated"));
});



var redisHost = builder.Configuration["Redis:Host"] ?? Environment.GetEnvironmentVariable("REDIS_HOST");
var redisPassword = builder.Configuration["Redis:Password"] ?? Environment.GetEnvironmentVariable("REDIS_PASSWORD");
var redisInstance = builder.Configuration["Redis:InstanceName"] ?? Environment.GetEnvironmentVariable("REDIS_INSTANCE");

var redisConfig = new ConfigurationOptions
{
    EndPoints = { $"{redisHost}" },
    Password = redisPassword,

    AbortOnConnectFail = false,

    ConnectTimeout = 50,   
    SyncTimeout   = 50,    
    AsyncTimeout  = 50,       
    ReconnectRetryPolicy = new LinearRetry(50),

    KeepAlive = 5,

    // PENTING → nonaktifkan connect backoff
    BacklogPolicy = BacklogPolicy.FailFast
};


var mux = ConnectionMultiplexer.Connect(redisConfig);

builder.Services.AddSingleton<IConnectionMultiplexer>(mux);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = redisConfig;
    options.InstanceName = redisInstance;
});
builder.Services.AddHostedService<MqttRecoveryService>();
builder.Services.AddHostedService<RedisRecoveryService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BleTracking API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(MstBuildingProfile));
builder.Services.AddScoped<IMstBuildingService, MstBuildingService>();
builder.Services.AddScoped<IMstFloorService, MstFloorService>();
builder.Services.AddScoped<IMstFloorplanService, MstFloorplanService>();
builder.Services.AddScoped<IFloorplanMaskedAreaService, FloorplanMaskedAreaService>();
builder.Services.AddScoped<MstBuildingRepository>();
builder.Services.AddScoped<MstFloorRepository>();
builder.Services.AddScoped<MstFloorplanRepository>();
builder.Services.AddScoped<FloorplanMaskedAreaRepository>();
builder.Services.AddScoped<FloorplanDeviceRepository>();

builder.Services.AddScoped<IGeofenceService, GeofenceService>();
builder.Services.AddScoped<IBoundaryService, BoundaryService>();
builder.Services.AddScoped<IStayOnAreaService, StayOnAreaService>();
builder.Services.AddScoped<IOverpopulatingService, OverpopulatingService>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();


builder.Services.AddScoped<GeofenceRepository>();
builder.Services.AddScoped<StayOnAreaRepository>();
builder.Services.AddScoped<OverpopulatingRepository>();
builder.Services.AddScoped<BoundaryRepository>();

var port = Environment.GetEnvironmentVariable("MST_BUILDING_PORT") ?? "5010" ??
           builder.Configuration["Ports:MstBuildingService"];
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var host = env == "Production" ? "0.0.0.0" : "localhost";
builder.WebHost.UseUrls($"http://{host}:{port}");

    var app = builder.Build();

        app.MapGet("/hc", async (IServiceProvider sp) =>
    {
        var db = sp.GetRequiredService<BleTrackingDbContext>();
        // var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HealthCheck");

        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1"); // cek koneksi DB
             Log.Information("Health check passed - Database connected");
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
             Log.Error(ex, "Health check failed - Database unreachable");
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
var basePath = AppContext.BaseDirectory;
var uploadsPath = Path.Combine(basePath, "Uploads", "BuildingImages");
Directory.CreateDirectory(uploadsPath);


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads/BuildingImages"
});
// var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads/BuildingImages");
// Directory.CreateDirectory(uploadsPath);


// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(uploadsPath),
//     RequestPath = "/Uploads/BuildingImages"
// });


// // app.UseHttpsRedirection();
// app.UseRouting();
// app.UseApiKeyAuthentication();
// app.UseAuthentication();
// app.UseAuthorization();
// app.MapControllers();
// app.Run();

    // var timeoutInSeconds = builder.Configuration.GetValue<int>("RequestTimeout");

    app.UseCors("AllowAll");
    // app.UseHttpsRedirection();
    app.UseRouting();
    app.UseApiKeyAuthentication();
    app.UseAuthentication();
    app.UseAuthorization(); 
    // app.UseRateLimiter();
    // app.UseRequestTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
    // app.UseFixedWindowRateLimiter(150, TimeSpan.FromMinutes(1));
    app.MapControllers();
    app.Run();


















