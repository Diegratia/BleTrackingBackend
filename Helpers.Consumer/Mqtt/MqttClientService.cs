using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Helpers.Consumer.Mqtt
{
    public class MqttClientService : IMqttClientService, IDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;
        public event Func<string, string, Task>? OnMessageReceived;

        public MqttClientService(IConfiguration config)
        {
            _mqttClient = new MqttFactory().CreateMqttClient();

            var host = config["Mqtt:Host"] ?? "192.168.1.116";
            var port = int.Parse(config["Mqtt:Port"] ?? "1888");
            var username = config["Mqtt:Username"] ?? "bio_mqtt";
            var password = config["Mqtt:Password"] ?? "P@ssw0rd";
            var clientId = (config["Mqtt:ClientId"] ?? "Tracking-People-Backend") + "-" + Guid.NewGuid();

            _options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(host, port)
                .WithCredentials(username, password)
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;
            _mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine($"Connected to MQTT broker {host}:{port}");
            };
            _mqttClient.DisconnectedAsync += async e =>
            {
                Console.WriteLine("Disconnected from MQTT broker. Retrying...");
                await Task.Delay(2000);
                try { await _mqttClient.ConnectAsync(_options); }
                catch (Exception ex) { Console.WriteLine($"Reconnect failed: {ex.Message}"); }
            };
        }

        private async Task EnsureConnectedAsync()
        {
            if (!_mqttClient.IsConnected)
                await _mqttClient.ConnectAsync(_options);
        }

        public async Task PublishAsync(string topic, string payload, bool retain = false, int qos = 1)
        {
            await EnsureConnectedAsync();

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .Build();

            await _mqttClient.PublishAsync(message);
            Console.WriteLine($" Published to {topic}");
        }

        public async Task SubscribeAsync(string topic, int qos = 1)
        {
            await EnsureConnectedAsync();

            await _mqttClient.SubscribeAsync(
                new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                    .Build()
            );
            Console.WriteLine($"Subscribed to {topic}");
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            Console.WriteLine($"Received {topic} → {payload}");

            if (OnMessageReceived != null)
                await OnMessageReceived.Invoke(topic, payload);
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
        }
        
    }
}
