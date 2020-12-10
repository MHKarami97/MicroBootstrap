using System;
using MicroBootstrap.Queries.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.Queries
{
    public static class Extensions
    {
        public static IServiceCollection AddQueryHandlers(this IServiceCollection serviceCollection)
        {
            // Scan method is not built-in .net core DPI, but here we use scrutor that just existing .net core DPI with two extention method scand and decorate but we can use also autofac
            // https://github.com/khellang/Scrutor
            serviceCollection.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());
            return serviceCollection;
        }

        public static IServiceCollection AddInMemoryQueryDispatcher(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IQueryDispatcher, InMemoryQueryDispatcher>();
        }
    }
}
