using System;
using System.IO;
using MicroBootstrap.Serialization.Serializer;

namespace MicroBootstrap.Serialization
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

