using System.Threading.Tasks;

namespace MicroBootstrap.Events.Dispatchers
{
    public interface IEventDispatcher
    {
        Task PublishAsync<T>(T @event) where T : class, IEvent;
    }
}