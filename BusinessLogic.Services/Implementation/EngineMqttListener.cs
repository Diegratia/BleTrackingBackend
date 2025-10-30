using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Helpers.Consumer;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repositories.DbContexts;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementation
{
    public class EngineMqttListener : BackgroundService
    {
        private readonly IMqttClientService _mqttClientService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;

        public EngineMqttListener(IMqttClientService mqttClientService, IServiceScopeFactory scopeFactory, ILogger logger)
        {
            _mqttClientService = mqttClientService;
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _mqttClientService.SubscribeAsync("engine/#");
            _mqttClientService.OnMessageReceived += async (topic, payload) =>
        {
            try
            {

                var json = JsonDocument.Parse(payload);
                var status = json.RootElement.GetProperty("status").GetString()?.ToLowerInvariant();
                var timestamp = json.RootElement.GetProperty("timestamp").GetString()?.ToLowerInvariant();
                var engineId = json.RootElement.GetProperty("engineTrackingId").GetString()?.ToLowerInvariant();

                if (string.IsNullOrEmpty(status))
                {
                    Console.WriteLine($" No 'status' field found in payload for {engineId}");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var engineService = scope.ServiceProvider.GetRequiredService<IMstEngineService>();

                var dto = new MstEngineUpdateDto();

                switch (status)
                {
                    case "start":
                        dto.ServiceStatus = ServiceStatus.Start;
                        dto.IsLive = 0;
                        break;
                        
                    case "online":
                        dto.ServiceStatus = ServiceStatus.Online;
                        dto.IsLive = 1;
                        dto.LastLive = DateTime.Now;
                        break;
                        
                    case "heartbeat":
                        dto.IsLive = 1;
                        dto.LastLive = DateTime.Now;
                        break;

                    case "stop":
                        dto.ServiceStatus = ServiceStatus.Stop;
                        dto.LastLive = DateTime.Now;
                        dto.IsLive = 0;
                        break;

                    default:
                        Console.WriteLine($"Unknown engine status value: {status}");
                        return;
                }

                await engineService.UpdateEngineByIdAsync(engineId, dto);
                Console.WriteLine($"[{status}] updated {engineId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing MQTT message: {ex}");
            }
        };
            // Keep service alive
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
