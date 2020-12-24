namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Serialization
{
    public interface IRabbitMQSerializer
    {
        T Deserialize<T>(byte[] value);
        object Deserialize(byte[] value);
        byte[] Serialize<T>(T value);
        byte[] Serialize(object value);
    }
}