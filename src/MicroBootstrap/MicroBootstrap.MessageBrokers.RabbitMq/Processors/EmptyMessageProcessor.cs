
using System.Threading.Tasks;

namespace MicroBootstrap.MessageBrokers.RabbitMq.Processors
{
    internal class EmptyMessageProcessor : IMessageProcessor
    {
        public Task<bool> TryProcessAsync(string id) => Task.FromResult(true);

        public Task RemoveAsync(string id) => Task.CompletedTask;
    }
}