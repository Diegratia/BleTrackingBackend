using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

namespace BusinessLogic.Services.Implementation
{
    public class HealthWarmupService : IHostedService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HealthWarmupService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            var services = _configuration.GetSection("HealthCheck:Services")
                                         .Get<Dictionary<string, string>>() ?? new();

            var warmupTasks = services.Values.Select(url =>
                client.GetAsync(url, cancellationToken)
                      .ContinueWith(t => { }, TaskContinuationOptions.OnlyOnFaulted) // Abaikan error
            );

            await Task.WhenAll(warmupTasks);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}