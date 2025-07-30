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
    Env.Load("/app/.env");
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

builder.Services.AddDbContext<BleTrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BleTrackingDbConnection") ??
                         "Server=192.168.1.116,1433;Database=BleTrackingDb;User Id=sa;Password=Password_123#;TrustServerCertificate=True"));


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
    options.AddPolicy("RequirePrimaryOrSystemRole", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("Primary"));
    });
     options.AddPolicy("RequirePrimaryAdminOrSystemRole", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("PrimaryAdmin"));
    });
     options.AddPolicy("RequireAll", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("System") || context.User.IsInRole("PrimaryAdmin") || context.User.IsInRole("Primary"));
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


builder.Services.AddAutoMapper(typeof(VisitorProfile));

builder.Services.AddScoped<IVisitorService, VisitorService>();

// builder.Services.AddScoped<IMstBleReaderService, MstBleReaderService>();
// builder.Services.AddScoped<IFloorplanMaskedAreaService, FloorplanMaskedAreaService>();

builder.Services.AddScoped<VisitorRepository>();



var port = Environment.GetEnvironmentVariable("VISITOR_PORT") ?? "10019" ??
           builder.Configuration["Ports:VisitorService"];
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var host = env == "Production" ? "0.0.0.0" : "localhost";
builder.WebHost.UseUrls($"http://{host}:{port}");

var app = builder.Build();

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
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads/visitorFaceImages");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads/visitorFaceImages"
});
// app.UseHttpsRedirection();
app.UseRouting();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();






































































