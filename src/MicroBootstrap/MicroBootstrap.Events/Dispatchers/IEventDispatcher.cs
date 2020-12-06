using System.Threading.Tasks;
using MicroBootstrap.Messages;
using MicroBootstrap.RabbitMq;

namespace MicroBootstrap.Events.Dispatchers
{
    public interface IEventDispatcher
    {
        Task PublishAsync<T>(T @event) where T : class, IEvent;
    }
}