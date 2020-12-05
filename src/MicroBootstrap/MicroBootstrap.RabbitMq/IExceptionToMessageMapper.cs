using System;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    public interface IExceptionToMessageMapper
    {
        object Map(Exception exception, object message);
    }
}