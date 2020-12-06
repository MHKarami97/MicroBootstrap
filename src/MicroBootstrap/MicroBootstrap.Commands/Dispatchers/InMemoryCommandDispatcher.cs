using System.Threading.Tasks;
using Autofac;
using MicroBootstrap.Messages;
using MicroBootstrap.RabbitMq;

namespace MicroBootstrap.Commands.Dispatchers
{
    public class InMememoryCommandDispatcher : ICommandDispatcher
    {
        private readonly IComponentContext _context;

        public InMememoryCommandDispatcher(IComponentContext context)
        {
            _context = context;
        }

        public async Task SendAsync<T>(T command) where T : ICommand
            => await _context.Resolve<ICommandHandler<T>>().HandleAsync(command);
    }
}