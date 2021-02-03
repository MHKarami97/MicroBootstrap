
using System.Threading.Tasks;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Processors;

namespace MicroBootstrap.MessageBrokers.RabbitMQ.Processors
{
    internal class EmptyMessageProcessor : IMessageProcessor
    {
        public Task<bool> TryProcessAsync(string id) => Task.FromResult(true);

        public Task RemoveAsync(string id) => Task.CompletedTask;
    }
}