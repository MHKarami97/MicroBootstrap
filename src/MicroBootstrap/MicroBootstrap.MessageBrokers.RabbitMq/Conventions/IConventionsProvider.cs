using System;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions
{
    public interface IConventionsProvider
    {
        IConventions Get<T>() where T : class;
    }
}