using System;
using System.Threading;
using System.Threading.Tasks;
using Helpers.Consumer.Mqtt;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Background
{
    public class MqttPubBackgroundService : BackgroundService
    {
        private readonly MqttPubQueue _queue;
        private readonly IMqttClientService _mqtt;
        private readonly ILogger<MqttPubBackgroundService> _logger;

        public MqttPubBackgroundService(
            MqttPubQueue queue,
            IMqttClientService mqtt,
            ILogger<MqttPubBackgroundService> logger)
        {
            _queue = queue;
            _mqtt = mqtt;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[MqttPubBG] Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var evt = await _queue.DequeueAsync(stoppingToken);
                    if (evt == null) continue;

                    if (!_mqtt.IsConnected())
                    {
                        _logger.LogWarning("[MqttPubBG] MQTT offline, skip publish: {Topic}", evt.Topic);
                        continue;
                    }

                    await _mqtt.PublishAsync(evt.Topic, evt.Payload);
                    _logger.LogDebug("[MqttPubBG] Published: {Topic}", evt.Topic);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[MqttPubBG] Error processing queue");
                }
            }

            _logger.LogInformation("[MqttPubBG] Service stopped");
        }
    }
}
