using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RawRabbit;
using RawRabbit.Enrichers.MessageContext;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Publishers
{
    public class BusPublisher : IBusPublisher
    {
        private readonly IBusClient _busClient;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<BusPublisher> _logger;
        private readonly IConventionsProvider _conventionsProvider;

        public BusPublisher(IBusClient busClient, RabbitMqOptions options, ILogger<BusPublisher> logger,
            IConventionsProvider conventionsProvider)
        {
            _busClient = busClient;
            _options = options;
            _logger = logger;
            _conventionsProvider = conventionsProvider;
        }

        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null,
            IConventions messageConventions = null)
            where T : class
        {
            var _spanContextHeader = _options.GetSpanContextHeader();
            var _loggerEnabled = _options.Logger?.Enabled ?? true;

            return _busClient.PublishAsync(message, ctx =>
                ctx.UsePublishConfiguration(cfg => cfg.WithProperties(properties =>
                    {
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

                        var conventions = messageConventions ?? _conventionsProvider.Get<T>();

                        if (messageConventions is { })
                        {
                            cfg.WithRoutingKey(messageConventions.RoutingKey);
                            cfg.OnDeclaredExchange(x =>
                                x.WithType(RawRabbit.Configuration.Exchange.ExchangeType.Topic)
                                    .WithName(messageConventions.Exchange));
                        }

                        if (_loggerEnabled)
                        {
                            _logger.LogTrace($"Publishing a message with routing key: '{conventions.RoutingKey}' " +
                                             $"to exchange: '{conventions.Exchange}' " +
                                             $"[id: '{properties.MessageId}', correlation id: '{properties.CorrelationId}']");
                        }
                    }))
                    .UseMessageContext(messageContext));
        }
    }
}