using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class SerilogHostExtensions
    {
        /// <summary>
        /// Konfigurasi Serilog berbasis environment:
        /// - Development  -> Information
        /// - Production   -> Warning
        /// Support Docker & Windows Service
        /// </summary>
        public static WebApplicationBuilder UseSerilogExtension(this WebApplicationBuilder builder)
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var minimumLevel = envName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? LogEventLevel.Information
                : LogEventLevel.Warning;

            var serviceName = AppDomain.CurrentDomain.FriendlyName
                .Replace(".dll", "")
                .Replace(".exe", "")
                .ToLower();

            bool isDocker = Directory.Exists("/app");

            var logDir = isDocker
                ? "/app/logs"
                : Path.Combine(AppContext.BaseDirectory, $"logs_{serviceName}");

            Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(logDir, $"{serviceName}-log-.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", serviceName)
                .WriteTo.Console(
                    restrictedToMinimumLevel: minimumLevel,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Service} | {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    logFile,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    restrictedToMinimumLevel: minimumLevel,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Service} | {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.Host.UseSerilog();

            Console.WriteLine($"Serilog initialized");
            Console.WriteLine($"Environment     : {envName}");
            Console.WriteLine($"MinimumLevel    : {minimumLevel}");
            Console.WriteLine($"Log Directory   : {logDir}");

            return builder;
        }
    }
}
