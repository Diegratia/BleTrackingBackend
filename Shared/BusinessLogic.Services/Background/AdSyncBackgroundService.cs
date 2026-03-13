using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Background
{
    /// <summary>
    /// Background service for scheduled Active Directory synchronization
    /// Runs periodically to sync users from AD based on configured intervals
    /// </summary>
    public class AdSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdSyncBackgroundService> _logger;

        public AdSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AdSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AD Sync Background Service is starting");

            // Initial delay before first run
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingSyncs(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during AD sync background processing");
                }

                // Wait for 1 minute before checking again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("AD Sync Background Service is stopping");
        }

        private async Task ProcessPendingSyncs(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Repositories.DbContexts.BleTrackingDbContext>();
            var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();

            // Get all AD configs that are enabled and due for sync
            var now = DateTime.UtcNow;
            var configsQuery = dbContext.ActiveDirectoryConfigs
                .Where(x => x.Status != 0 && x.IsEnabled);

            var configs = await System.Threading.Tasks.Task.Run(() => configsQuery.ToList(), stoppingToken);

            foreach (var config in configs)
            {
                // Check if AD sync feature is enabled for this application
                bool featureEnabled = await featureService.IsFeatureEnabledAsync(
                    Shared.BusinessLogic.Services.Feature.FeatureDefinition.SaasActiveDirectory,
                    config.ApplicationId);

                if (!featureEnabled)
                {
                    continue; // Skip if feature not enabled
                }

                // Check if sync is due
                bool shouldSync = false;
                if (!config.LastSyncAt.HasValue)
                {
                    // Never synced - run now
                    shouldSync = true;
                }
                else
                {
                    var nextSyncDue = config.LastSyncAt.Value.AddMinutes(config.SyncIntervalMinutes);
                    if (now >= nextSyncDue)
                    {
                        shouldSync = true;
                    }
                }

                if (shouldSync)
                {
                    _logger.LogInformation($"Running scheduled AD sync for application {config.ApplicationId}");

                    try
                    {
                        // Create a scope for the sync operation
                        using var syncScope = _serviceProvider.CreateScope();
                        var adSyncService = syncScope.ServiceProvider.GetRequiredService<IAdSyncService>();

                        // Trigger sync
                        var result = await adSyncService.TriggerSyncAsync(new AdSyncTrigger
                        {
                            ForceFullSync = false
                        });

                        if (result.Success)
                        {
                            _logger.LogInformation($"Scheduled AD sync completed for application {config.ApplicationId}: {result.UsersSynced} users synced");
                        }
                        else
                        {
                            _logger.LogWarning($"Scheduled AD sync failed for application {config.ApplicationId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error during scheduled AD sync for application {config.ApplicationId}");
                    }
                }
            }
        }
    }
}
