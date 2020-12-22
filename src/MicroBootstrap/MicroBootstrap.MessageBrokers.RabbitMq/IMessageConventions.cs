using System;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    public interface IMessageConventions
    {
        Type Type { get; }
        string RoutingKey { get; }
        string Exchange { get; }
        string Queue { get; }
    }
}
