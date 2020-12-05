using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.Commands;
using MicroBootstrap.Events;
using MicroBootstrap.Messages;

namespace MicroBootstrap.RabbitMq
{
    public interface IBusPublisher
    {
        Task SendAsync<TCommand>(TCommand command, ICorrelationContext context)
            where TCommand : ICommand;

        Task PublishAsync<TEvent>(TEvent @event, ICorrelationContext context)
            where TEvent : IEvent;

        Task PublishAsync<T>(T message, string messageId = null, string correlationId = null, string spanContext = null,
         object messageContext = null, IDictionary<string, object> headers = null) where T : class;
    }
}