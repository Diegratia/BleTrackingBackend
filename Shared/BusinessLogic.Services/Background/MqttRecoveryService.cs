using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer.Mqtt;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Background
{
        public class MqttRecoveryService : BackgroundService
    {
        private readonly IMqttClientService _mqtt;
        private readonly ILogger<MqttRecoveryService> _logger;

        public MqttRecoveryService(IMqttClientService mqtt, ILogger<MqttRecoveryService> logger)
        {
            _mqtt = mqtt;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(120000, stoppingToken);

                    if (_mqtt.IsConnected())
                    continue;
                    _logger.LogWarning("MQTT down — recovery attempt...");
                    await _mqtt.TryReconnect();
            }
        }
    }


}