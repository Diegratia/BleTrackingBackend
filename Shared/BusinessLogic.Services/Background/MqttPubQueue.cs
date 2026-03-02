using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Background
{
    public class MqttPubQueue : IMqttPubQueue
    {
        private readonly Channel<MqttPubEvent> _channel;

        public MqttPubQueue()
        {
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            };
            _channel = Channel.CreateBounded<MqttPubEvent>(options);
        }

        public void Enqueue(string topic, string payload)
        {
            _channel.Writer.TryWrite(new MqttPubEvent
            {
                Topic = topic,
                Payload = payload
            });
        }

        public async ValueTask<MqttPubEvent?> DequeueAsync(CancellationToken ct)
        {
            return await _channel.Reader.ReadAsync(ct);
        }
    }
}
