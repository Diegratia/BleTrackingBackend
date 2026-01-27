using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Shared.Contracts;
using Data.ViewModels;

namespace BusinessLogic.Services.Implementation.EngineService
{
    public class EngineStatusMonitor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // ⏱️ cek tiap 30 detik
        private readonly int _offlineThresholdSeconds = 90; // ❗ mark offline jika > 90 detik tanpa heartbeat

        public EngineStatusMonitor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

while (!stoppingToken.IsCancellationRequested)
{
    try
    {
        using var scope = _scopeFactory.CreateScope();
        var engineService = scope.ServiceProvider.GetRequiredService<IMstEngineService>();

        var engines = await engineService.GetAllOnlineAsync() ?? new List<MstEngineDto>();
        var now = DateTime.UtcNow;

        foreach (var engine in engines)
        {
            if (engine.LastLive == null)
                continue;

            var diff = now - engine.LastLive.Value.ToUniversalTime();
            if (diff.TotalSeconds > _offlineThresholdSeconds)
            {
                Console.WriteLine($"⚠️ Engine {engine.EngineTrackingId} offline ({diff.TotalSeconds:F0}s ago)");
                await engineService.UpdateEngineByIdAsync(engine.EngineTrackingId, new Data.ViewModels.MstEngineUpdateDto
                {
                    ServiceStatus = ServiceStatus.Offline,
                    IsLive = 0,
                    LastLive = now
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in EngineStatusMonitor: {ex}");
    }

    await Task.Delay(_checkInterval, stoppingToken);
}

            Console.WriteLine("🛑 EngineStatusMonitor stopped.");
        }
    }
}
