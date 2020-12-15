using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers;
using RawRabbit;
using RawRabbit.Enrichers.MessageContext;

namespace MicroBootstrap.MessageBrokers.RabbitMq
{
    public class BusPublisher : IBusPublisher
    {
        private readonly IBusClient _busClient;

        public BusPublisher(IBusClient busClient)
        {
            _busClient = busClient;
        } 
        //UseMessageContext is part of RawRabbit that and it serialize as a header in RabbitMQ properties
        //ICorrelationContext is metadata comes with message and use in message flow and we will not mutate this
        //ICorrelationContext and we create this object in the begining of inside APIGateway and let it go with the
        //messages and use this ICorrelationContext class in message handlers as a second parameter
        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
            where T : class
        {
            return _busClient.PublishAsync(message, ctx => ctx.UseMessageContext(messageContext));
        }
    }
}