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
using Repositories.Seeding;
using Entities.Models;
using DotNetEnv;

try
{
    Env.Load("././.env");
    Console.WriteLine("Successfully loaded .env file from /app/.env");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load .env file: {ex.Message}");
}

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
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

builder.Services.AddDbContext<BleTrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BleTrackingDbConnection") ??
                         "Server=192.168.1.116,1433;Database=TrackingBleDevV4;User Id=sa;Password=Password_123#;TrustServerCertificate=True"));

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
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(AuthProfile));
builder.Services.AddScoped<IAuthService, AuthService>();

// var port = Environment.GetEnvironmentVariable("AUTH_PORT") ?? "5001";
// var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
// var host = env == "Production" ? "0.0.0.0" : "localhost";
// builder.WebHost.UseUrls($"http://{host}:{port}");

var port = Environment.GetEnvironmentVariable("AUTH_PORT") ??
           builder.Configuration["Ports:AuthService"] ?? "5001";
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var host = env == "Production" ? "0.0.0.0" : "localhost";
builder.WebHost.UseUrls($"http://{host}:{port}");

var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
//     try
//     {
//         // Console.WriteLine("Applying migrations...");
//         // context.Database.Migrate(); 
//         // Console.WriteLine("Migrations applied successfully.");

//         Console.WriteLine("Seeding database...");
//         DatabaseSeeder.Seed(context);
//         Console.WriteLine("Database seeded successfully.");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error during migration or seeding: {ex.Message}");
//         throw; 
//     }
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API V1");
        c.RoutePrefix = "";
    });
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// app.MapGet("/api/Auth/health", () => "Health Check");
// app.MapGet("/health", () => "Health Check");


// Console.WriteLine("Environment Variables Check");
// Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {env}");
// Console.WriteLine($"AUTH_PORT: {Environment.GetEnvironmentVariable("AUTH_PORT")}");
// Console.WriteLine($"Configured Port: {port}");
// Console.WriteLine($"Host: {host}");
// Console.WriteLine($"Application URL: http://{host}:{port}");
// Console.WriteLine($"Current Environment: {app.Environment.EnvironmentName}");
// Console.WriteLine($"Is Development: {app.Environment.IsDevelopment()}");
// Console.WriteLine($"Connection String: {builder.Configuration.GetConnectionString("TrackingBleDbConnection")}");
// Console.WriteLine($"Starting on http://{host}:{port} in {env} environment...");

// Console.WriteLine($"Starting on http://{host}:{port} in {env} environment...");
// Console.WriteLine($"Jwt:Issuer = {builder.Configuration["Jwt:Issuer"]}");
// Console.WriteLine($"Jwt:Audience = {builder.Configuration["Jwt:Audience"]}");
// Console.WriteLine($"Jwt:Key = {builder.Configuration["Jwt:Key"]}");

app.Run();