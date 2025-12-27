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
// Konfigurasi CORS
builder.Services.AddCorsExtension();

// Konfigurasi sumber konfigurasi
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Konfigurasi Controllers
builder.Services.AddControllers();

// Registrasi otomatis validasi FluentValidation
// Scan semua validator di assembly yang mengandung BrandValidator
builder.Services.AddValidatorExtensions();

// Konfigurasi DbContext
builder.Services.AddDbContextExtension(builder.Configuration);

// Konfigurasi AutoMapper
builder.Services.AddAutoMapper(typeof(BusinessLogic.Services.Extension.AuthProfile));

// Konfigurasi Autentikasi JWT
builder.Services.AddJwtAuthExtension(builder.Configuration);

// Konfigurasi Otorisasi
builder.Services.AddAuthorizationPolicies();

// Konfigurasi Swagger
builder.Services.AddSwaggerExtension();

// Konfigurasi IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrasi Services

builder.Services.AddAutoMapper(typeof(BlacklistAreaProfile));

builder.Services.AddScoped<IBlacklistAreaService, BlacklistAreaService>();

// builder.Services.AddScoped<IFloorplanMaskedAreaService, FloorplanMaskedAreaService>();
// builder.Services.AddScoped<IVisitorService, VisitorService>();

builder.Services.AddScoped<BlacklistAreaRepository>();
// Konfigurasi port dan host
builder.UseDefaultHostExtension("BLACKLIST_AREA_PORT", "5020");

var app = builder.Build();

app.UseHealthCheckExtension();

// Inisialisasi Database (opsional: migrasi atau seeding)
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
app.UseRouting();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();