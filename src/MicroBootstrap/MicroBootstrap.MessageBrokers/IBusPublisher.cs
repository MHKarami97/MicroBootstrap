using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers.RabbitMQ;

namespace MicroBootstrap.MessageBrokers
{
    public interface IBusPublisher
    {
        Task PublishAsync<T>(T message, string messageId = null, string correlationId = null, string spanContext = null,
            object messageContext = null, IDictionary<string, object> headers = null,
            IMessageConventions messageConventions = null) where T : class;
    }
}