using System.Collections.Generic;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Context
{
    public interface IContextProvider1
    {
        string HeaderName { get; }

        object Get(IDictionary<string, object> headers);
    }

    public interface IContextProvider
    {
        string HeaderName { get; }
        object Get(IDictionary<string, object> headers);
    }
}