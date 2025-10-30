using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Repositories.DbContexts;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Implementation;
using Repositories.Repository;
using Repositories.Seeding;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
// using BusinessLogic.Services.Extension;
// using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Extension.RootExtension;
// using BusinessLogic.Services.Extension.RootExtension;

try
{
    Env.Load("/app/.env");
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
builder.Services.AddJwtAuthExtension();

// Konfigurasi DbContext
builder.Services.AddDbContextExtension(builder.Configuration);

// Konfigurasi AutoMapper
builder.Services.AddAutoMapper(typeof(AuthProfile));

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