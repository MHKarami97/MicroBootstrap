using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.MessageBrokers.Outbox.Configurators
{
    internal sealed class MessageOutboxConfigurator : IMessageOutboxConfigurator
    {
        public IServiceCollection Services { get; }
        public OutboxOptions Options { get; }

        public MessageOutboxConfigurator(IServiceCollection services, OutboxOptions options)
        {
            Services = services;
            Options = options;
        }
    }
}