using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public class RequestTimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _timeout;

        public RequestTimeoutMiddleware(RequestDelegate next, TimeSpan timeout)
        {
            _next = next;
            _timeout = timeout;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestTask = _next(context);
            var timeoutTask = Task.Delay(_timeout);

            var completed = await Task.WhenAny(requestTask, timeoutTask);

            if (completed == timeoutTask)
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        success = false,
                        msg = "Request timed out",
                        collection = new { data = (object?)null },
                        code = 408
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
                return; // jangan tunggu requestTask
            }

            await requestTask; // kalau request selesai duluan
        }
    }

    public static class RequestTimeoutMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTimeout(
            this IApplicationBuilder builder,
            TimeSpan timeout)
        {
            return builder.UseMiddleware<RequestTimeoutMiddleware>(timeout);
        }
    }
}
