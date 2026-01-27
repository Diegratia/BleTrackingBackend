using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public class RateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimiter _limiter;
        private readonly TimeSpan _window;

        public RateLimiterMiddleware(RequestDelegate next, RateLimiter limiter, TimeSpan window)
        {
            _next = next;
            _limiter = limiter;
            _window = window;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var lease = await _limiter.AcquireAsync(1);

            if (!lease.IsAcquired)
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";
                    context.Response.Headers["Retry-After"] = ((int)_window.TotalSeconds).ToString();

                    var response = new
                    {
                        success = false,
                        msg = "Too many requests. Please try again later.",
                        collection = new { data = (object?)null },
                        code = 429,
                        retryAfterSeconds = (int)_window.TotalSeconds
                    };

                    var json = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(json);
                }

                return;
            }

            await _next(context);
        }
    }

    public static class RateLimiterMiddlewareExtensions
    {
        public static IApplicationBuilder UseFixedWindowRateLimiter(
            this IApplicationBuilder builder,
            int permitLimit,
            TimeSpan window)
        {
            var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });

            return builder.UseMiddleware<RateLimiterMiddleware>(limiter, window);
        }
    }
}
