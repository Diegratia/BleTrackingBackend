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


var builder = WebApplication.CreateBuilder(args);
// Console.WriteLine($"Jwt:Issuer = {builder.Configuration["Jwt:Issuer"]}");
// Console.WriteLine($"Jwt:Audience = {builder.Configuration["Jwt:Audience"]}");
// Console.WriteLine($"Jwt:Key = {builder.Configuration["Jwt:Key"]}");


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() 
              .AllowAnyMethod() 
              .AllowAnyHeader(); 
    });
});


builder.Services.AddAutoMapper(typeof(MstIntegrationProfile));


builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
  options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Token = accessToken.Substring("Bearer ".Length).Trim();
        }
        return Task.CompletedTask;
    },
    OnAuthenticationFailed = context =>
    {
        Console.WriteLine("Authentication failed: " + context.Exception.Message);
        Console.WriteLine("Token: " + context.Request.Headers["Authorization"]);
        return Task.CompletedTask;
    },
    OnTokenValidated = context =>
    {
        Console.WriteLine("Token validated successfully");
        return Task.CompletedTask;
    }
};
    });



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TrackingBle API", Version = "v1" });

    // detail token
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("BleTrackingConnectionString")));
 
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//         };
//     });


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IMstIntegrationService, MstIntegrationService>();
builder.Services.AddScoped<IMstBrandService, MstBrandService>();
builder.Services.AddScoped<IMstApplicationService, MstApplicationService>();

builder.Services.AddScoped<MstIntegrationRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
    // context.Database.Migrate();
    // DatabaseSeeder.Seed(context);
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrackingBle API v1");
        c.RoutePrefix = string.Empty; 
    });
}

// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
//     RequestPath = "/Uploads"
// });
app.UseCors("AllowAll");


app.UseHttpsRedirection();

app.UseRouting();
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



