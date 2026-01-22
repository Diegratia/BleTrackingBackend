using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;  // ✅ Gunakan IHostEnvironment, bukan IWebHostEnvironment
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Data.ViewModels.ResponseHelper;
using DataView;

namespace Data.ViewModels.Shared.ExceptionHelper  // ✅ Pastikan namespace sama
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;  // ✅ Changed to IHostEnvironment

        public CustomExceptionMiddleware(
            RequestDelegate next,
            ILogger<CustomExceptionMiddleware> logger,
            IHostEnvironment env)  // ✅ Changed to IHostEnvironment
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            object result;
            int statusCode;

            switch (exception)
            {
                case NotFoundException ex:  // ✅ Now available
                    statusCode = 404;
                    result = ApiResponse.NotFound(ex.Message);  // ✅ Now available
                    _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
                    break;

                case BusinessException ex:  // ✅ Now available
                    statusCode = 400;
                    result = ApiResponse.BadRequest(ex.Message);  // ✅ Now available
                    _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
                    break;

                case ValidationException ex:  // ✅ Now available
                    statusCode = 400;
                    result = ApiResponse.BadRequest(ex.Message, ex.Errors);  // ✅ Now available
                    _logger.LogWarning(ex, "Validation failed: {Message}", ex.Message);
                    break;

                case UnauthorizedException ex:  // ✅ Now available
                    statusCode = 401;
                    result = ApiResponse.Unauthorized(ex.Message);  // ✅ Now available
                    _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                    break;

                case UnauthorizedAccessException ex:
                    statusCode = 403;
                    result = ApiResponse.Forbidden("Access denied");  // ✅ Now available
                    _logger.LogWarning(ex, "Forbidden access");
                    break;

                case KeyNotFoundException ex:
                    statusCode = 404;
                    // result = ApiResponse.NotFound("Resource not found");  // ✅ Now available
                    result = ApiResponse.NotFound(ex.Message);  // ✅ Use ex.Message
                    _logger.LogWarning(ex, "Key not found");
                    break;

                case ArgumentNullException ex:  // ✅ Add this
                    statusCode = 400;
                    result = ApiResponse.BadRequest($"Parameter '{ex.ParamName}' is required");
                    _logger.LogWarning(ex, "Null argument");
                    break;

                case ArgumentException ex:
                    statusCode = 400;
                    result = ApiResponse.BadRequest(ex.Message);  // ✅ Now available
                    _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
                    break;

                case InvalidOperationException ex:  // ✅ Add this
                    statusCode = 400;
                    result = ApiResponse.BadRequest(ex.Message);
                    _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                    break;

                case DbUpdateException ex:
                    statusCode = 400;
                    var dbMessage = _env.IsDevelopment() ? ex.InnerException?.Message : "Database operation failed";
                    result = ApiResponse.BadRequest(dbMessage ?? "Database error");  // ✅ Now available
                    _logger.LogError(ex, "Database error");
                    break;

                default:
                    statusCode = 500;
                    var message = _env.IsDevelopment() ? exception.Message : "Internal server error";
                    result = ApiResponse.InternalError(message);  // ✅ Now available
                    _logger.LogError(exception, "Unhandled exception");
                    break;
            }

            response.StatusCode = statusCode;
            // await response.WriteAsync(JsonSerializer.Serialize(result));
                await response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // ✅ Consistent naming
            }));
        }
    }
}