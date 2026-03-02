using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class HostExtensions
    {
        /// <summary>
        /// Konfigurasi port & host berdasarkan environment variable dan appsettings.json.
        /// </summary>
        /// <param name="builder">WebApplicationBuilder</param>
        /// <param name="defaultPort">Port default jika tidak ada env/config</param>
        /// <param name="portKey">Nama env key untuk port (misal: AUTH_PORT)</param>
        /// <returns>Builder dengan URL sudah diset</returns>
        public static WebApplicationBuilder UseDefaultHostExtension(this WebApplicationBuilder builder, string portKey, string defaultPort = "5000")
        {
            var envPort = Environment.GetEnvironmentVariable(portKey);
            var configPort = builder.Configuration[$"Ports:{portKey}"];
            var port = envPort ?? configPort ?? defaultPort;

            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var host = envName == "Production" ? "0.0.0.0" : "localhost";

            builder.WebHost.UseUrls($"http://{host}:{port}");

            Console.WriteLine($"Running on: http://{host}:{port}");
            return builder;
        }
    }
}
