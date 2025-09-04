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
        private const string KeyField = "X-BIOPEOPLETRACKING-API-KEY";
        private const string QueryParamKey = "apiKey";

        public ApiKeyMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip middleware for specific endpoints
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.Contains("/export", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/refresh", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/public", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/fill-invitation-form", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            string apiKeyValue = null;

            // Check for API key in header
            if (context.Request.Headers.TryGetValue(KeyField, out var headerApiKey))
            {
                apiKeyValue = headerApiKey.ToString();
            }
            // Fallback to query parameter if header is not found
            else if (context.Request.Query.TryGetValue(QueryParamKey, out var queryApiKey))
            {
                apiKeyValue = queryApiKey.ToString();
            }

            // Validate API key
            if (string.IsNullOrEmpty(apiKeyValue))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is missing or empty\", \"collection\": { \"data\": null }, \"code\": 401}");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
            var integration = await dbContext.MstIntegrations
                .FirstOrDefaultAsync(i => i.ApiKeyValue == apiKeyValue && i.Status != 0 && i.ApiTypeAuth == ApiTypeAuth.ApiKey);

            if (integration == null)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\": false, \"msg\": \"Invalid API Key\", \"collection\": { \"data\": null }, \"code\": 401}");
                return;
            }

            context.Items["MstIntegration"] = integration;

            await _next(context);
        }
    }





    // public class ApiKeyMiddleware
    // {
    //     private readonly RequestDelegate _next;
    //     private readonly IServiceProvider _serviceProvider;

    //     public ApiKeyMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    //     {
    //         _next = next;
    //         _serviceProvider = serviceProvider;
    //     }

    //     public async Task InvokeAsync(HttpContext context)
    //     {
    //         var KeyField = "X-BIOPEOPLETRACKING-API-KEY";

    //         // Skip middleware for specific endpoints
    //         if (context.Request.Path.Value.Contains("/export", StringComparison.OrdinalIgnoreCase) ||
    //             context.Request.Path.Value.Contains("/refresh", StringComparison.OrdinalIgnoreCase) ||
    //             context.Request.Path.Value.Contains("/public", StringComparison.OrdinalIgnoreCase) ||
    //             context.Request.Path.Value.Contains("/fill-invitation-form", StringComparison.OrdinalIgnoreCase))
    //         {
    //             await _next(context);
    //             return;
    //         }

    //         // Check for X-BIOPEOPLETRACKING-API-KEY header
    //         if (!context.Request.Headers.TryGetValue(KeyField, out var apiKeyValues))
    //         {
    //             context.Response.StatusCode = 401;
    //             await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is missing\", \"collection\": { \"data\": null }, \"code\": 401}");
    //             return;
    //         }

    //         var KeyValue = apiKeyValues.ToString();
    //         if (string.IsNullOrEmpty(KeyValue))
    //         {
    //             context.Response.StatusCode = 401;
    //             await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is empty\", \"collection\": { \"data\": null }, \"code\": 401}");
    //             return;
    //         }

    //         using var scope = _serviceProvider.CreateScope();
    //         var dbContext = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
    //         var integration = await dbContext.MstIntegrations
    //             .FirstOrDefaultAsync(i => i.ApiKeyValue == KeyValue && i.Status != 0 && i.ApiTypeAuth == ApiTypeAuth.ApiKey);

    //         if (integration == null)
    //         {
    //             context.Response.StatusCode = 401;
    //             await context.Response.WriteAsync("{\"success\": false, \"msg\": \"Invalid API Key\", \"collection\": { \"data\": null }, \"code\": 401}");
    //             return;
    //         }

    //         context.Items["MstIntegration"] = integration;

    //         await _next(context);
    //     }
    // }






