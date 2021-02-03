using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.MessageBrokers.Outbox
{
    public interface IMessageOutboxConfigurator
    {
        IServiceCollection Services { get; }
        OutboxOptions Options { get; }
    }
}