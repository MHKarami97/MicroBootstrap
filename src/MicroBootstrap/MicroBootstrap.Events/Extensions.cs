using System;
using MicroBootstrap.Events.Dispatchers;
using MicroBootstrap.Types;
using Microsoft.Extensions.DependencyInjection;
namespace MicroBootstrap.Events
{
    public static class Extensions
    {
        public static IServiceCollection AddInMemoryEventDispatcher(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IEventDispatcher, InMemoryEventDispatcher>();
        }

        public static IServiceCollection AddEventHandlers(this IServiceCollection serviceCollection)
        {
            serviceCollection.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>))
                  .WithoutAttribute(typeof(DecoratorAttribute)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
            return serviceCollection;
        }
    }
}