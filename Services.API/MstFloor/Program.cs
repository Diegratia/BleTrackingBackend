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


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();



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
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<BleTrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BleTrackingDbConnection") ));


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
    options.AddPolicy("RequireSecondaryRole", policy =>
        policy.RequireRole("Secondary"));

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
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("PrimaryAdmin") || context.User.IsInRole("Primary" ) || context.User.IsInRole("Secondary" ));
    });
    options.AddPolicy("RequireUserCreatedRole", policy =>
        policy.RequireRole("UserCreated"));
});

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


builder.Services.AddAutoMapper(typeof(MstFloorProfile));
builder.Services.AddScoped<IMstFloorService, MstFloorService>();
builder.Services.AddScoped<IMstFloorplanService, MstFloorplanService>();
builder.Services.AddScoped<IFloorplanMaskedAreaService, FloorplanMaskedAreaService>();



builder.Services.AddScoped<MstFloorRepository>();
builder.Services.AddScoped<MstFloorplanRepository>();
builder.Services.AddScoped<FloorplanMaskedAreaRepository>();
builder.Services.AddScoped<FloorplanDeviceRepository>();

builder.Services.AddScoped<IGeofenceService, GeofenceService>();
builder.Services.AddScoped<IBoundaryService, BoundaryService>();
builder.Services.AddScoped<IStayOnAreaService, StayOnAreaService>();
builder.Services.AddScoped<IOverpopulatingService, OverpopulatingService>();

builder.Services.AddScoped<GeofenceRepository>();
builder.Services.AddScoped<StayOnAreaRepository>();
builder.Services.AddScoped<OverpopulatingRepository>();
builder.Services.AddScoped<BoundaryRepository>();




var port = Environment.GetEnvironmentVariable("MST_FLOOR_PORT") ??
           builder.Configuration["Ports:MstFloorService"] ?? "5013";
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var host = env == "Production" ? "0.0.0.0" : "localhost";
builder.WebHost.UseUrls($"http://{host}:{port}");

    var app = builder.Build();

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



// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
//     RequestPath = "/Uploads"
// });

app.UseCors("AllowAll");
// // Buat direktori Uploads/FloorImages jika belum ada
// var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads/FloorImages");
// Directory.CreateDirectory(uploadsPath);

// // Sajikan file statis di /Uploads/FloorImages/
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(uploadsPath),
//     RequestPath = "/Uploads/FloorImages"
// });


// app.UseHttpsRedirection();
app.UseRouting();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();






















































