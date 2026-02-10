namespace BusinessLogic.Services.Background
{
    public interface IMqttPubQueue
    {
        void Enqueue(string topic, string payload);
    }
}
