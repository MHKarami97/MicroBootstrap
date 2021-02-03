using System;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions
{
    public class Conventions : IConventions
    {
        public Type Type { get; }
        public string RoutingKey { get; }
        public string Exchange { get; }
        public string Queue { get; }

        public Conventions(Type type, string routingKey, string exchange, string queue)
        {
            Type = type;
            RoutingKey = routingKey;
            Exchange = exchange;
            Queue = queue;
        }
    }
}