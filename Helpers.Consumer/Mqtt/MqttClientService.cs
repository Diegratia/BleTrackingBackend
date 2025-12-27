using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Helpers.Consumer.Mqtt
{
    public class MqttClientService : IMqttClientService, IDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;
        private readonly ILogger<MqttClientService> _logger;
        private static readonly string _clientId = "PEOPLE-TRACKING-API-" + Guid.NewGuid();

        public event Func<string, string, Task>? OnMessageReceived;

        public MqttClientService(IConfiguration config, ILogger<MqttClientService> logger)
        {
            _logger = logger;
            _mqttClient = new MqttFactory().CreateMqttClient();

            var host = config["Mqtt:Host"]??Environment.GetEnvironmentVariable("MQTT_HOST");;
            var port = int.Parse(config["Mqtt:Port"]?? Environment.GetEnvironmentVariable("MQTT_PORT"));
            var username = config["Mqtt:Username"]?? Environment.GetEnvironmentVariable("MQTT_USERNAME");
            var password = config["Mqtt:Password"]?? Environment.GetEnvironmentVariable("MQTT_PASSWORD");

            _options = new MqttClientOptionsBuilder()
                .WithClientId(_clientId)
                .WithTcpServer(host, port)
                .WithCredentials(username, password)
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;

            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("[MQTT] Connected, subscribing engine/#");
                await _mqttClient.SubscribeAsync("engine/#");
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("[MQTT] Disconnected");
            };

            _ = TryConnectStartupAsync();
        }

        public bool IsConnected() => _mqttClient.IsConnected;

        private async Task TryConnectStartupAsync()
        {
            try
            {
                await _mqttClient.ConnectAsync(_options);
                _logger.LogInformation("[MQTT] Startup connect OK");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[MQTT] Startup connect failed: {ex.Message}");
            }
        }

        public async Task TryReconnect()
        {
            try
            {
                await _mqttClient.ConnectAsync(_options);
                _logger.LogInformation("[MQTT] Reconnected");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[MQTT] Reconnect failed: {ex.Message}");
            }
        }

        public async Task SubscribeAsync(string topic, int qos = 1)
        {
            if (!IsConnected())
            {
                _logger.LogWarning($"[MQTT] offline, cannot subscribe {topic}");
                return;
            }

            await _mqttClient.SubscribeAsync(
                new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
                    .Build()
            );

            _logger.LogInformation($"[MQTT] Subscribed: {topic}");
        }

            public async Task PublishAsync(string topic, string payload)
            {
                try
                {
                    if (!IsConnected())
                    {
                        _logger.LogWarning($"[MQTT] offline, skip publish {topic}");
                        return;
                    }

                    await _mqttClient.PublishAsync(
                        new MqttApplicationMessageBuilder()
                            .WithTopic(topic)
                            .WithPayload(payload)
                            .Build()
                    );

                    _logger.LogInformation($"[MQTT] Published: {topic}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[MQTT] Failed to publish topic: {topic}");
                }
            }


        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            if (OnMessageReceived != null)
                await OnMessageReceived.Invoke(topic, payload);
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
        }
    }

}
