using System;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions
{
    public interface IConventions
    {
        Type Type { get; }
        string RoutingKey { get; }
        string Exchange { get; }
        string Queue { get; }
    }
}