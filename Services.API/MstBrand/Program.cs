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
    using FluentValidation;
    using FluentValidation.AspNetCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using BusinessLogic.Services.Extension.RootExtension;


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
    // Registrasi otomatis validasi FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    // Scan semua validator di assembly yang mengandung BrandValidator
    builder.Services.AddValidatorsFromAssemblyContaining<MstBrandCreateDtoValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<MstBrandUpdateDtoValidator>();


    builder.Services.AddDbContext<BleTrackingDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("BleTrackingDbConnection") ?? "Server= 192.168.1.116,5433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));

    builder.Services.AddAutoMapper(typeof(MstBrandProfile));

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

    builder.Services.AddScoped<IMstBrandService, MstBrandService>();
    builder.Services.AddScoped<MstBrandRepository>();

    // builder.Services.AddRateLimiter(options =>
    // {
    //     options.AddFixedWindowLimiter("fixed", opt =>
    //     {
    //         opt.Window = TimeSpan.FromMinutes(1);
    //         opt.PermitLimit = 150;
    //         opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    //         opt.QueueLimit = 0;
    //     });

    //     options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    //     options.OnRejected = async (context, token) =>
    //     {
    //         var httpResponse = context.HttpContext.Response;
    //         httpResponse.StatusCode = StatusCodes.Status429TooManyRequests;
    //         httpResponse.ContentType = "application/json";

    //         // Optional: tambahkan header supaya client tahu kapan bisa retry
    //         httpResponse.Headers["Retry-After"] = ((int)TimeSpan.FromMinutes(1).TotalSeconds).ToString();

    //         var response = new
    //         {
    //             success = false,
    //             msg = "Too many requests. Please try again later.",
    //             collection = new { data = (object?)null },
    //             code = 429
    //         };

    //         var json = System.Text.Json.JsonSerializer.Serialize(response);

    //         await httpResponse.WriteAsync(json, token);
    //     };
    // });


    var port = Environment.GetEnvironmentVariable("MST_BRAND_PORT") ?? "5009" ??
            builder.Configuration["Ports:MstBrandService"];
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var host = env == "Production" ? "0.0.0.0" : "localhost";
    builder.WebHost.UseUrls($"http://{host}:{port}");

    var app = builder.Build();
    
    app.MapGet("/hc", () => Results.Ok(new
    {
        code = 200,
        msg = "Health Check",
    }))
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
    var timeoutInSeconds = builder.Configuration.GetValue<int>("RequestTimeout");

    app.UseCors("AllowAll");
    // app.UseHttpsRedirection();
    app.UseRouting();
    app.UseApiKeyAuthentication();
    app.UseAuthentication();
    app.UseAuthorization(); 
    // app.UseRateLimiter();
    app.UseRequestTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
    // app.MapControllers().RequireRateLimiting("fixed");
    app.UseFixedWindowRateLimiter(150, TimeSpan.FromMinutes(1));
    app.MapControllers();
    app.Run();



