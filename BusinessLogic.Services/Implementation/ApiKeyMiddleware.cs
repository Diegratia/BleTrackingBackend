using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Helpers.Consumer;

public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApiKeyMiddleware>();
        }
    }

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public ApiKeyMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

     public async Task InvokeAsync(HttpContext context)
    {
        var KeyField = "X-API-KEY-TRACKING-PEOPLE";
        var apiUrl = "http://192.168.1.116:10000";
        // Periksa header X-API-KEY-TRACKING-PEOPLE untuk semua endpoint
        if (!context.Request.Headers.TryGetValue(KeyField, out var apiKeyValues))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is missing\", \"collection\": { \"data\": null }, \"code\": 401}");
            return;
        }
        Console.Write(apiKeyValues);

        var KeyValue = apiKeyValues.ToString(); 
        if (string.IsNullOrEmpty(KeyValue))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is empty\", \"collection\": { \"data\": null }, \"code\": 401}");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
        var integration = await dbContext.MstIntegrations
            .FirstOrDefaultAsync(i => i.ApiKeyField == KeyField && i.ApiKeyValue == KeyValue && i.Status != 0 && i.ApiUrl == apiUrl && i.ApiTypeAuth == ApiTypeAuth.ApiKey);

        if (integration == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("{\"success\": false, \"msg\": \"Invalid API Key\", \"collection\": { \"data\": null }, \"code\": 401}");
            return;
        }

        context.Items["MstIntegration"] = integration;

        await _next(context);
    }
    
        // public async Task InvokeAsync(HttpContext context)
        // {
        //     var whitelistedPaths = new[]
        //     {
        //         "/api/MstFloor/export/pdf", 
        //         "/health",                  
        //         "/swagger"                 
        //     };

        //     var requestPath = context.Request.Path.Value?.ToLower();

        //     if (whitelistedPaths.Any(p => requestPath.StartsWith(p)))
        //     {
        //         await _next(context); // Skip API key check
        //         return;
        //     }

        //     var KeyField = "X-API-KEY-TRACKING-PEOPLE";
        //     var apiUrl = "http://localhost:10013";

        //     if (!context.Request.Headers.TryGetValue(KeyField, out var apiKeyValues))
        //     {
        //         context.Response.StatusCode = 401;
        //         await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is missing\", \"collection\": { \"data\": null }, \"code\": 401}");
        //         return;
        //     }

        //     var KeyValue = apiKeyValues.ToString();
        //     if (string.IsNullOrEmpty(KeyValue))
        //     {
        //         context.Response.StatusCode = 401;
        //         await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is empty\", \"collection\": { \"data\": null }, \"code\": 401}");
        //         return;
        //     }

        //     using var scope = _serviceProvider.CreateScope();
        //     var dbContext = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
        //     var integration = await dbContext.MstIntegrations
        //         .FirstOrDefaultAsync(i => i.ApiKeyField == KeyField && i.ApiKeyValue == KeyValue && i.Status != 0 && i.ApiUrl == apiUrl && i.ApiTypeAuth == ApiTypeAuth.ApiKey);

        //     if (integration == null)
        //     {
        //         context.Response.StatusCode = 401;
        //         await context.Response.WriteAsync("{\"success\": false, \"msg\": \"Invalid API Key\", \"collection\": { \"data\": null }, \"code\": 401}");
        //         return;
        //     }

        //     context.Items["MstIntegration"] = integration;

        //     await _next(context);
        // }


    }



