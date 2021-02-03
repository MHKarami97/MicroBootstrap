using System.Threading.Tasks;

namespace MicroBootstrap.Commands.Dispatchers
{
    public interface ICommandDispatcher
    {
        Task SendAsync<T>(T command) where T : class, ICommand;
    }
}