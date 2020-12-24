using System.Collections.Generic;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Serialization;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Context
{
    internal sealed class ContextProvider : IContextProvider
    {
        private readonly IRabbitMQSerializer _serializer;
        public string HeaderName { get; }

        public ContextProvider(IRabbitMQSerializer serializer, RabbitMqOptions options)
        {
            _serializer = serializer;
            HeaderName = string.IsNullOrWhiteSpace(options.Context?.Header)
                ? "message_context"
                : options.Context.Header;
        }

        public object Get(IDictionary<string, object> headers)
        {
            if (headers is null)
            {
                return null;
            }
            
            if (!headers.TryGetValue(HeaderName, out var context))
            {
                return null;
            }

            if (!(context is byte[] bytes))
            {
                return null;
            }
            var result = _serializer.Deserialize(bytes);
            
            return result;
        }
    }
}