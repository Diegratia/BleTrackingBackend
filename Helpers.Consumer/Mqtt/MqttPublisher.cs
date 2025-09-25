using MQTTnet;

using System.Text;
using Microsoft.Extensions.Configuration;

namespace Helpers.Consumer.Mqtt
{
    public class MqttPublisher : IMqttPublisher
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;

        public MqttPublisher(IConfiguration config)
        {
            _mqttClient = new MqttClientFactory().CreateMqttClient();

            var brokerHost = config["Mqtt:Host"] ?? "192.168.1.116";
            var brokerPort = int.Parse(config["Mqtt:Port"] ?? "1888");
            var username = config["Mqtt:Username"] ?? "bio_mqtt";
            var password = config["Mqtt:Password"] ?? "P@ssw0rd";
            var clientId = config["Mqtt:ClientId"] + "-" + Guid.NewGuid() ?? Guid.NewGuid().ToString();
            // var clientId = config["Mqtt:ClientId"] ?? Guid.NewGuid().ToString();

            _options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(brokerHost, brokerPort)
                .WithCredentials(username, password)
                .Build();
        }

        public async Task PublishAsync(string topic, string payload, bool retain = false, int qos = 1)
        {
            if (!_mqttClient.IsConnected)
                await _mqttClient.ConnectAsync(_options);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
                .Build();

            await _mqttClient.PublishAsync(message);
        }
    }
}