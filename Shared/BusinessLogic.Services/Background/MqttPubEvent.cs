using System;

namespace BusinessLogic.Services.Background
{
    public class MqttPubEvent
    {
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    }
}
