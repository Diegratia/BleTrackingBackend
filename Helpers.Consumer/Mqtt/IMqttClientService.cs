using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpers.Consumer.Mqtt
{
    public interface IMqttClientService
    {
        Task PublishAsync(string topic, string payload);
        Task SubscribeAsync(string topic, int qos = 1);
        event Func<string, string, Task>? OnMessageReceived;
        bool IsConnected();
        Task TryReconnect();

    }
}

