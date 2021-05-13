using System;
using System.IO;

namespace MicroBootstrap.Serializer
{
    public interface ISerializer
    {
        object Deserialize(Stream data, Type objectType);
        void Serialize(object value, Stream output);
    }

    public interface ITextSerializer : ISerializer { }

    public static class DefaultSerializer
    {
        public static ISerializer Instance { get; set; } = new SystemTextJsonSerializer();
    }
}

