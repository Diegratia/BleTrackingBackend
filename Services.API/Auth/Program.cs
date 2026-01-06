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
using Microsoft.Extensions.Hosting;
using BusinessLogic.Services.Extension.RootExtension;
using Data.ViewModels.Shared.ExceptionHelper;


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
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrasi Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserGroupRepository>();
builder.Services.AddScoped<MstIntegrationRepository>();
builder.Services.AddScoped<RefreshTokenRepository>();

// service email
builder.Services.AddScoped<IEmailService, EmailService>();
// Konfigurasi port dan host
builder.UseDefaultHostExtension("AUTH_PORT", "5001");
builder.Host.UseWindowsService();


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
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseRouting();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();