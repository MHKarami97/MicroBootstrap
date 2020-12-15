using System.Threading.Tasks;
using MicroBootstrap.Commands;
using MicroBootstrap.Commands.Dispatchers;
using MicroBootstrap.Events;
using MicroBootstrap.Events.Dispatchers;

namespace MicroBootstrap.MessageBrokers
{
    internal sealed class ServiceBusMessageDispatcher : ICommandDispatcher, IEventDispatcher
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ICorrelationContextAccessor _accessor;

        public ServiceBusMessageDispatcher(IBusPublisher busPublisher, ICorrelationContextAccessor accessor)
        {
            _busPublisher = busPublisher;
            _accessor = accessor;
        }

        public Task SendAsync<T>(T command) where T : class, ICommand
            => _busPublisher.SendAsync(command, _accessor.CorrelationContext);

        public Task PublishAsync<T>(T @event) where T : class, IEvent
            => _busPublisher.PublishAsync(@event, _accessor.CorrelationContext);
    }
}