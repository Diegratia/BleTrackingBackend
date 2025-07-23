using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            // Periksa header X-API-Key untuk semua endpoint
            if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("{\"success\": false, \"msg\": \"API Key is missing\", \"collection\": { \"data\": null }, \"code\": 401}");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
            var integration = await dbContext.MstIntegrations
                .FirstOrDefaultAsync(i => i.ApiKeyField == "X-API-Key" && i.ApiKeyValue == apiKey && i.Status != 0);

            if (integration == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("{\"success\": false, \"msg\": \"Invalid API Key\", \"collection\": { \"data\": null }, \"code\": 401}");
                return;
            }

            // Simpan informasi integrasi di HttpContext untuk digunakan di controller jika perlu
            context.Items["Integration"] = integration;

            await _next(context);
        }
    }



