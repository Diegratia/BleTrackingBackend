using Shared.Config;

namespace BusinessLogic.Services.Extension.RootExtension
{
    /// <summary>
    /// Extension for loading environment configuration.
    /// Delegates to Shared.Config.ConfigLoader.
    /// </summary>
    public static class EnvTryCatchExtension
    {
        /// <summary>
        /// Load .env configuration from embedded resource or file.
        /// Priority: Embedded Resource (production) > File (development)
        /// </summary>
        public static void LoadEnvWithTryCatch()
        {
            ConfigLoader.LoadConfig();
        }
    }
}
