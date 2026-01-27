using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class AddRedisExtensions
    {
        public static IServiceCollection AddRedisExtension(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var redisHost = configuration["Redis:Host"] 
                            ?? Environment.GetEnvironmentVariable("REDIS_HOST");

            var redisPassword = configuration["Redis:Password"] 
                                ?? Environment.GetEnvironmentVariable("REDIS_PASSWORD");

            var redisInstance = configuration["Redis:InstanceName"] 
                                ?? Environment.GetEnvironmentVariable("REDIS_INSTANCE");

            if (string.IsNullOrWhiteSpace(redisHost))
            {
                throw new Exception("Redis host is not configured");
            }

            var redisConfig = new ConfigurationOptions
            {
                EndPoints = { redisHost },
                Password = redisPassword,

                AbortOnConnectFail = false,

                ConnectTimeout = 50,
                SyncTimeout = 50,
                AsyncTimeout = 50,

                ReconnectRetryPolicy = new LinearRetry(50),
                KeepAlive = 5,

                // penting: nonaktifkan connect backoff
                BacklogPolicy = BacklogPolicy.FailFast
            };

            var mux = ConnectionMultiplexer.Connect(redisConfig);

            mux.ConnectionFailed += (_, e) =>
            {
                Log.Warning("Redis connection failed: {FailureType}", e.FailureType);
            };

            mux.ConnectionRestored += (_, _) =>
            {
                Log.Information("Redis connection restored");
            };

            services.AddSingleton<IConnectionMultiplexer>(mux);

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = redisConfig;
                options.InstanceName = redisInstance;
            });

            return services;
        }
    }
}
