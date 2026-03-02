using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Shared.Config
{
    public static class ConfigLoader
    {
        /// <summary>
        /// Load .env to environment variables (for backward compatibility)
        /// </summary>
        public static void LoadConfig()
        {
            try
            {
                bool loaded = false;

                // Priority 1: Embedded .env resource (production)
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = assembly.GetManifestResourceNames()
                        .FirstOrDefault(n => n.EndsWith(".env"));

                    if (resourceName != null)
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        using (var reader = new StreamReader(stream))
                        {
                            var envContent = reader.ReadToEnd();

                            foreach (var line in envContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var trimmedLine = line.Trim();
                                if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith("#"))
                                {
                                    var parts = trimmedLine.Split('=', 2);
                                    if (parts.Length == 2)
                                    {
                                        Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                                    }
                                }
                            }
                            Console.WriteLine("Loading embedded .env resource...");
                            loaded = true;
                        }
                    }
                }
                catch { /* No embedded resource */ }

                // Priority 2: File .env (development)
                if (!loaded)
                {
                    var possiblePaths = new[]
                    {
                        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                        Path.Combine(Directory.GetCurrentDirectory(), "../../.env"),
                        Path.Combine(AppContext.BaseDirectory, ".env"),
                        "/app/.env"
                    };

                    var envFile = possiblePaths.FirstOrDefault(File.Exists);
                    if (envFile != null)
                    {
                        Console.WriteLine($"Loading env file: {envFile}");
                        DotNetEnv.Env.Load(envFile);
                        loaded = true;
                    }
                }

                if (!loaded)
                {
                    Console.WriteLine("No .env found - using appsettings.json only");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load config: {ex.Message}");
            }
        }

        /// <summary>
        /// Get appsettings.json content as stream (for embedded resource)
        /// Returns null if file doesn't exist as embedded resource
        /// </summary>
        public static Stream? GetAppsettingsStream()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase));

                if (resourceName != null)
                {
                    var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        Console.WriteLine("Using embedded appsettings.json...");
                        return stream;
                    }
                }
            }
            catch { /* No embedded resource */ }

            return null;
        }

        /// <summary>
        /// Add appsettings.json to ConfigurationBuilder
        /// Usage: builder.Config.AddAppsettings();
        /// </summary>
        public static IConfigurationBuilder AddAppsettings(this IConfigurationBuilder builder)
        {
            // Try embedded resource first
            var stream = GetAppsettingsStream();
            if (stream != null)
            {
                // Copy to MemoryStream so we can dispose the original stream
                using (stream)
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    builder.AddJsonStream(ms);
                }
                return builder;
            }

            // Fallback to file (development)
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "../../appsettings.json"),
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                "/app/appsettings.json"
            };

            var appsettingsFile = possiblePaths.FirstOrDefault(File.Exists);
            if (appsettingsFile != null)
            {
                Console.WriteLine($"Using appsettings file: {appsettingsFile}");
                builder.AddJsonFile(appsettingsFile, optional: false, reloadOnChange: true);
            }
            else
            {
                Console.WriteLine("Warning: No appsettings.json found");
            }

            return builder;
        }
    }
}
