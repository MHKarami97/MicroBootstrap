using System.Threading.Tasks;
using Autofac;
using MicroBootstrap.Messages;
using MicroBootstrap.RabbitMq;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.Commands.Dispatchers
{
    public class InMememoryCommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceScopeFactory _serviceFactory;

        public InMememoryCommandDispatcher(IServiceScopeFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }
        public async Task SendAsync<T>(T command) where T : class, ICommand
        {
            // https://www.blog.jamesmichaelhickey.com/NET-Core-Dependency-Injection/
            using var scope = _serviceFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();
            await handler.HandleAsync(command);
        }
    }
}