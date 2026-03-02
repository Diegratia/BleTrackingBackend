using Shared.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Repositories.DbContexts;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Implementation;
using Microsoft.Extensions.FileProviders;
using BusinessLogic.Services.Interface;
using Repositories.Repository;
using Entities.Models;
using Repositories.Seeding;
using DotNetEnv;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Authorization;
using BusinessLogic.Services.Extension.RootExtension;
using Data.ViewModels.Shared.ExceptionHelper;
using Serilog;
using System.Text.Json.Serialization;

EnvTryCatchExtension.LoadEnvWithTryCatch();



var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddAppsettings();
builder.UseSerilogExtension();   
builder.Host.UseWindowsService();
builder.Host.UseSerilog();
builder.Services.AddCorsExtension();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
        options.JsonSerializerOptions.Converters.Add(
            new UtcDateTimeConverter()
        );
    });

builder.Services.AddDbContextExtension(builder.Configuration);

builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSwaggerExtension();

builder.Services.AddSwaggerExtension();


builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(TimeBlockGroupProfile));
builder.Services.AddAutoMapper(typeof(CardGroupProfile));
builder.Services.AddAutoMapper(typeof(CardAccessProfile));
// Registrasi Services\
// builder.Services.AddScoped<ITimeBlockService, TimeBlockService>();
builder.Services.AddScoped<ITimeGroupService, TimeGroupService>();
builder.Services.AddScoped<ITimeBlockService, TimeBlockService>();
builder.Services.AddScoped<ICardGroupService, CardGroupService>();
builder.Services.AddScoped<ICardAccessService, CardAccessService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddSingleton<MqttPubQueue>();
builder.Services.AddSingleton<IMqttPubQueue>(sp => sp.GetRequiredService<MqttPubQueue>());
builder.Services.AddHostedService<MqttPubBackgroundService>();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();
// builder.Services.AddScoped<ICardService, CardService>();

// Registrasi Repositories
builder.Services.AddScoped<TimeGroupRepository>();
builder.Services.AddScoped<TimeBlockRepository>();
builder.Services.AddScoped<CardGroupRepository>();
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<CardAccessRepository>();
builder.Services.AddScoped<FloorplanMaskedAreaRepository>();
builder.Services.AddScoped<TimeGroupRepository>();
builder.Services.AddScoped<MstMemberRepository>();


builder.UseDefaultHostExtension("CARD_ACCESS_PORT", "5028");

var app = builder.Build();
app.UseHealthCheckExtension();

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


app.UseCors("AllowAll");
// // app.UseHttpsRedirection();
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseRouting();
app.UseSerilogRequestLoggingExtension();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


