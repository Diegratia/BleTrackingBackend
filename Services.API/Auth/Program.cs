using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Repositories.DbContexts;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Extension;
using Repositories.Repository;
using Repositories.Seeding;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Konfigurasi sumber konfigurasi
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Konfigurasi Controllers
builder.Services.AddControllers();

    // Registrasi otomatis validasi FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    // Scan semua validator di assembly yang mengandung BrandValidator
    builder.Services.AddValidatorsFromAssemblyContaining<MstBrandCreateDtoValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<MstBrandUpdateDtoValidator>();

// Konfigurasi DbContext
builder.Services.AddDbContext<BleTrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BleTrackingDbConnection") ?? "Server= 192.168.1.116,5433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));

// Konfigurasi AutoMapper
builder.Services.AddAutoMapper(typeof(AuthProfile));

// Konfigurasi Autentikasi JWT
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
        // options.Events = new JwtBearerEvents
        // {
        //     OnAuthenticationFailed = context =>
        //     {
        //         Console.WriteLine("Authentication failed: " + context.Exception.Message);
        //         return Task.CompletedTask;
        //     },
        //     OnTokenValidated = context =>
        //     {
        //         Console.WriteLine("Token validated successfully");
        //         return Task.CompletedTask;
        //     }
        // };
    });

// Konfigurasi Otorisasi
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

    options.AddPolicy("RequirePrimaryOrSystemOrSuperAdminRole", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("Primary"));
    });
    options.AddPolicy("RequirePrimaryAdminOrSystemOrSuperAdminRole", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("PrimaryAdmin"));
    });
   options.AddPolicy("RequireAll", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("PrimaryAdmin") || context.User.IsInRole("Primary" ) || context.User.IsInRole("Secondary" ));
    });
    options.AddPolicy("RequireUserCreatedRole", policy =>
        policy.RequireRole("UserCreated"));
});

// Konfigurasi Swagger
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

// Konfigurasi IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrasi Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrasi Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserGroupRepository>();
builder.Services.AddScoped<RefreshTokenRepository>();
// builder.Services.AddScoped<VisitorRepository>();
// service email
builder.Services.AddScoped<IEmailService, EmailService>();
// Konfigurasi port dan host
var port = Environment.GetEnvironmentVariable("AUTH_PORT") ?? "5001" ??
           builder.Configuration["Ports:AuthService"] ;
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var host = env == "Production" ? "0.0.0.0" : "localhost";
builder.WebHost.UseUrls($"http://{host}:{port}");

var app = builder.Build();

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