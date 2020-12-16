using System.Threading.Tasks;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(string id);
        Task RemoveAsync(string id);
    }
}