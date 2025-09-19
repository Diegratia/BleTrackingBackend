using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpers.Consumer.Mqtt
{
    public interface IMqttPublisher
    {
        Task PublishAsync(string topic, string payload, bool retain = false, int qos = 1);
    }
}

