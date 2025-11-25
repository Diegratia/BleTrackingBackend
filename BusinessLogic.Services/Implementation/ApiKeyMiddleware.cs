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
        // private static readonly DateTime StaticExpiredDate = new DateTime(2025, 11, 01, 0, 0, 0, DateTimeKind.Utc);


        public ApiKeyMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

        public async Task InvokeAsync(HttpContext context)
        {
           //skip
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.Contains("/export", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/refresh", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/public", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/hc", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/integration-login", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/fill-invitation-form", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            string apiKeyValue = null;

            if (context.Request.Headers.TryGetValue(KeyField, out var headerApiKey))
            {
                apiKeyValue = headerApiKey.ToString();
            }
            else if (context.Request.Query.TryGetValue(QueryParamKey, out var queryApiKey))
            {
                apiKeyValue = queryApiKey.ToString();
            }

            if (string.IsNullOrEmpty(apiKeyValue))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is missing or empty\", \"collection\": { \"data\": null }, \"code\": 401}");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
        // var integration = await dbContext.MstIntegrations
        //     .FirstOrDefaultAsync(i => i.ApiKeyValue == apiKeyValue
        //     && i.Status != 0
        //     && i.ApiTypeAuth == ApiTypeAuth.ApiKey);
            var integration = await dbContext.MstIntegrations
            .Include(i => i.Application)
            .FirstOrDefaultAsync(i => i.ApiKeyValue == apiKeyValue && 
                                    i.Status != 0 && 
                                    i.ApiTypeAuth == ApiTypeAuth.ApiKey);

            if (integration == null)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"success\": false, \"msg\": \"Invalid API Key\", \"collection\": { \"data\": null }, \"code\": 401}");
            return;
        }

        // application expired
        if (integration.Application == null || integration.Application.ApplicationExpired < DateTime.UtcNow)
        {
            var expiredAt = integration.Application?.ApplicationExpired
                .ToString("yyyy-MM-ddTHH:mm:ssZ")?? "Unknown";

            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                $"{{\"success\": false, \"msg\": \"Application license expired at {expiredAt}\", \"collection\": {{ \"data\": null }}, \"code\": 403}}"
            );
            return;
        }

        // static expired 
        // if (DateTime.UtcNow > StaticExpiredDate)
        // {
        //     var expiredAt = StaticExpiredDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

        //     context.Response.StatusCode = 403;
        //     context.Response.ContentType = "application/json";
        //     await context.Response.WriteAsync(
        //         $"{{\"success\": false, \"msg\": \"Application license expired at {expiredAt}\", \"collection\": {{ \"data\": null }}, \"code\": 403}}"
        //     );

        //     return;
        // }

            context.Items["MstIntegration"] = integration;

            await _next(context);
        }
    }





