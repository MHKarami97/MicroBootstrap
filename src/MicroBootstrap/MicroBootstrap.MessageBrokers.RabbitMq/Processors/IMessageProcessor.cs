using System.Threading.Tasks;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Processors
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(string id);
        Task RemoveAsync(string id);
    }
}