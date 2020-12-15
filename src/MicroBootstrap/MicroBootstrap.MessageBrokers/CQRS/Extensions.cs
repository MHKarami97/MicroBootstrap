using System;
using System.Threading.Tasks;
using MicroBootstrap.Commands;
using MicroBootstrap.Commands.Dispatchers;
using MicroBootstrap.Events;
using MicroBootstrap.Events.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.MessageBrokers
{
    public static class Extensions
    {
        public static Task SendAsync<TCommand>(this IBusPublisher busPublisher, TCommand command, object messageContext)
            where TCommand : class, ICommand
            => busPublisher.PublishAsync(command, messageContext: messageContext);

        public static Task PublishAsync<TEvent>(this IBusPublisher busPublisher, TEvent @event, object messageContext)
            where TEvent : class, IEvent
            => busPublisher.PublishAsync(@event, messageContext: messageContext);

        public static IBusSubscriber SubscribeCommand<T>(this IBusSubscriber busSubscriber) where T : class, ICommand
            => busSubscriber.Subscribe<T>(async (serviceProvider, command, obj) =>
            {
                using var scope = serviceProvider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>().HandleAsync(command);
            });

        public static IBusSubscriber SubscribeEvent<T>(this IBusSubscriber busSubscriber) where T : class, IEvent
            => busSubscriber.Subscribe<T>(async (serviceProvider, @event, obj) =>
            {
                using var scope = serviceProvider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IEventHandler<T>>().HandleAsync(@event);
            });

        public static IServiceCollection AddServiceBusCommandDispatcher(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICommandDispatcher, ServiceBusMessageDispatcher>();
            return serviceCollection;
        }

        public static IServiceCollection AddServiceBusEventDispatcher(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IEventDispatcher, ServiceBusMessageDispatcher>();
            return serviceCollection;
        }
    }
}