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





