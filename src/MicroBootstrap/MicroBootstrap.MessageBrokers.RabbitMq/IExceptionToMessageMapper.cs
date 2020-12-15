using System;

namespace MicroBootstrap.MessageBrokers.RabbitMq
{
    public interface IExceptionToMessageMapper
    {
        object Map(Exception exception, object message);
    }
}