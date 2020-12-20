using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    public class BusPublisher : IBusPublisher
    {
        private readonly IBusClient _busClient;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<BusPublisher> _logger;
        private readonly INamingConventions _conventions;

        public BusPublisher(IBusClient busClient, RabbitMqOptions options, ILogger<BusPublisher> logger, INamingConventions conventions)
        {
            _busClient = busClient;
            this._options = options;
            this._logger = logger;
            this._conventions = conventions;
        }
        //UseMessageContext is part of RawRabbit that and it serialize as a header in RabbitMQ properties
        //ICorrelationContext is metadata comes with message and use in message flow and we will not mutate this
        //ICorrelationContext and we create this object in the begining of inside APIGateway and let it go with the
        //messages and use this ICorrelationContext class in message handlers as a second parameter
        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
            where T : class
        {
            var _spanContextHeader = _options.GetSpanContextHeader();
            var _loggerEnabled = _options.Logger?.Enabled ?? false;
            //TODO: handle other input parameters
            return _busClient.PublishAsync(message, ctx =>
            ctx.UsePublishConfiguration(cfg => cfg.WithProperties(properties =>
            {
                //properties.Persistent = _persistMessages;
                properties.MessageId = string.IsNullOrWhiteSpace(messageId)
                    ? Guid.NewGuid().ToString("N")
                    : messageId;
                properties.CorrelationId = string.IsNullOrWhiteSpace(correlationId)
                    ? Guid.NewGuid().ToString("N")
                    : correlationId;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Headers = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(spanContext))
                {
                    properties.Headers.Add(_spanContextHeader, spanContext);
                }

                if (headers is { })
                {
                    foreach (var (key, value) in headers)
                    {
                        if (string.IsNullOrWhiteSpace(key) || value is null)
                        {
                            continue;
                        }

                        properties.Headers.TryAdd(key, value);
                    }
                }
                var exchangeName = _conventions.ExchangeNamingConvention.Invoke(message.GetType());
                var routingKey = _conventions.RoutingKeyConvention.Invoke(message.GetType());
                if (_loggerEnabled)
                {
                    _logger.LogTrace($"Publishing a message with routing key: '{routingKey}' " +
                                     $"to exchange: '{exchangeName}' " +
                                     $"[id: '{properties.MessageId}', correlation id: '{properties.CorrelationId}']");
                }

            }))
            .UseMessageContext(messageContext));
        }
    }
}