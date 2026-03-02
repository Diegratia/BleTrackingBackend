using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Mendaftarkan endpoint "/hc" untuk health check database dan koneksi dasar.
        /// </summary>
        public static WebApplication UseHealthCheckExtension(this WebApplication app)
        {
            app.MapGet("/hc", async (IServiceProvider sp) =>
            {
                var db = sp.GetRequiredService<BleTrackingDbContext>();
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HealthCheck");

                try
                {
                    await db.Database.ExecuteSqlRawAsync("SELECT 1");
                    return Results.Ok(new
                    {
                        code = 200,
                        msg = "Healthy",
                        details = new { database = "Connected" }
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Health check failed");
                    return Results.Problem("Database unreachable", statusCode: 500);
                }
            })
            .AllowAnonymous();

            return app;
        }
    }
}
