using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DotNetEnv;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class EnvTryCatchExtension
    {
        public static void LoadEnvWithTryCatch()
        {
            try
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
                    Env.Load(envFile);
                }
                else
                {
                    Console.WriteLine("No .env file found — skipping load");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load .env file: {ex.Message}");
            }
        }
    }
}
