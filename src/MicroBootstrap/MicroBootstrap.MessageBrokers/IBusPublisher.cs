using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions;

namespace MicroBootstrap.MessageBrokers
{
    public interface IBusPublisher
    {
        Task PublishAsync<T>(T message, string messageId = null, string correlationId = null, string spanContext = null,
            object messageContext = null, IDictionary<string, object> headers = null,
            IConventions messageConventions = null) where T : class;
    }
}