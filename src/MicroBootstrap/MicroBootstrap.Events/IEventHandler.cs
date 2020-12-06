using MicroBootstrap.RabbitMq;
using System.Threading.Tasks;

namespace MicroBootstrap.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event);
    }
}