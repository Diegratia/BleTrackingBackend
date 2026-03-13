using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using BusinessLogic.Services.Interface;
using Data.ViewModels.ResponseHelper;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class LicenseMiddlewareExtensions
    {
        public static IApplicationBuilder UseLicenseValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LicenseMiddleware>();
        }
    }

    public class LicenseMiddleware
    {
        private readonly RequestDelegate _next;

        public LicenseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // 1. Skip validation for license activation and other essential paths
            // This allows users to activate a new license even if the current one is expired.
            if (IsExemptPath(path))
            {
                await _next(context);
                return;
            }

            // 2. Resolve ILicenseService from the request-scoped container
            // Middleware is singleton, but ILicenseService is Scoped.
            var licenseService = context.RequestServices.GetRequiredService<ILicenseService>();

            // 3. Perform global license check
            if (!await licenseService.IsLicenseValidAsync())
            {
                context.Response.StatusCode = 402; // Payment Required
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    msg = "Your application license has expired or is invalid. Please contact administrator to activate a valid license.",
                    code = 402,
                    collection = new { data = (object)null }
                });
                return;
            }

            await _next(context);
        }

        private bool IsExemptPath(string path)
        {
            return path.Contains("/api/license/activate", StringComparison.OrdinalIgnoreCase) ||
                   path.Contains("/api/license/machine-id", StringComparison.OrdinalIgnoreCase) ||
                   path.Contains("/api/license/info", StringComparison.OrdinalIgnoreCase) ||
                   path.Contains("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
                   path.Contains("/hc", StringComparison.OrdinalIgnoreCase) || // Health checks
                   path.Contains("/public", StringComparison.OrdinalIgnoreCase);
        }
    }
}
