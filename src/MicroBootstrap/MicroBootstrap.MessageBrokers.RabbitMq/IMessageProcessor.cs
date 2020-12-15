using System.Threading.Tasks;

namespace MicroBootstrap.MessageBrokers.RabbitMq
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(string id);
        Task RemoveAsync(string id);
    }
}