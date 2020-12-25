using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Context;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RawRabbit;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Publishers
{
    public class BusPublisher : IBusPublisher
    {
        private readonly IBusClient _busClient;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<BusPublisher> _logger;
        private readonly IConventionsProvider _conventionsProvider;
        private readonly IContextProvider _contextProvider;
        private readonly IRabbitMQSerializer _serializer;

        public BusPublisher(IBusClient busClient, RabbitMqOptions options, ILogger<BusPublisher> logger,
            IConventionsProvider conventionsProvider, IContextProvider contextProvider, IRabbitMQSerializer serializer)
        {
            _serializer = serializer;
            _busClient = busClient;
            _contextProvider = contextProvider;
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
            var _contextHeader = _options.GetContextHeader();
            
            var _loggerEnabled = _options.Logger?.Enabled ?? true;
            var _contextEnabled = _options.Context?.Enabled ?? true;

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

                        if (_contextEnabled)
                        {
                            // add context to header
                            IncludeMessageContext(messageContext, properties);
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
                    })));
        }

        private void IncludeMessageContext(object context, IBasicProperties properties)
        {
            if (context is { })
            {
                properties.Headers.Add(_contextProvider.HeaderName, _serializer.Serialize(context));
                return;
            }

            properties.Headers.Add(_contextProvider.HeaderName, "{}");
        }
    }
}