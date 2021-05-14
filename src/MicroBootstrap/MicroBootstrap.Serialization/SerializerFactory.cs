using MicroBootstrap.Serialization.Serializer;

namespace MicroBootstrap.Serialization
{
    public static class SerializerFactory
    {
        public static ISerializer Create(SerializerType type)
        {
            switch (type)
            {
                case SerializerType.SystemTextJson:
                    return DefaultSerializer.Instance;
                case SerializerType.JsonNet:
                    return new JsonNetSerializer();
                case SerializerType.Utf8Json:
                    return new Utf8JsonSerializer();
                case SerializerType.MessagePack:
                    return new MessagePackSerializer();
                default:
                    return DefaultSerializer.Instance;
            }
        }
    }
}