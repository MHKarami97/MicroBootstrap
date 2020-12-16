using System;
using System.Reflection;
using MicroBootstrap.Messages;
using RawRabbit.Common;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    internal sealed class CustomNamingConventions : NamingConventions
    {
        private readonly RabbitMqOptions _options;
        private readonly bool _snakeCase;
        private readonly string _queueTemplate;
        public CustomNamingConventions(RabbitMqOptions options)
        {
            _options = options;
            _queueTemplate = string.IsNullOrWhiteSpace(_options.Queue.Template)
                ? "{{assembly}}/{{exchange}}.{{message}}"
                : options.Queue.Template;
            _snakeCase = options.ConventionsCasing == null ? true : options.ConventionsCasing?.Equals("snakeCase", StringComparison.InvariantCultureIgnoreCase) == true;
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            ExchangeNamingConvention = type => GetExchangeName(type);
            RoutingKeyConvention = type => GetRoutingKey(type);
            QueueNamingConvention = type => GetQueue(type);
            ErrorExchangeNamingConvention = () => $"{options?.Exchange?.Name}.error";
            RetryLaterExchangeConvention = span => $"{options?.Exchange?.Name}.retry";
            RetryLaterQueueNameConvetion = (exchange, span) =>
                $"{options?.Exchange?.Name}.retry_for_{exchange.Replace(".", "_")}_in_{span.TotalMilliseconds}_ms"
                    .ToLowerInvariant();
        }

        private string GetRoutingKey(Type type)
        {
            var attribute = GeAttribute(type);
            var routingKey = string.IsNullOrWhiteSpace(attribute?.RoutingKey) ? type.Name : attribute.RoutingKey;

            return ApplySnakeCasing(routingKey);
        }

        private string GetExchangeName(Type type)
        {
            var attribute = GeAttribute(type);
            var exchange = string.IsNullOrWhiteSpace(attribute?.Exchange)
                ? string.IsNullOrWhiteSpace(_options.Exchange?.Name) ?
                _options.Exchange.Name : type.Assembly.GetName().Name
                : attribute.Exchange;

            return ApplySnakeCasing(exchange);
        }
        public string GetQueue(Type type)
        {
            var attribute = GeAttribute(type);
            if (!string.IsNullOrWhiteSpace(attribute?.Queue))
            {
                return ApplySnakeCasing(attribute.Queue);
            }
            var assembly = type.Assembly.GetName().Name;
            var message = type.Name;
            var exchange = string.IsNullOrWhiteSpace(attribute?.Exchange) ? _options.Exchange.Name : attribute.Exchange;
            var queue = _queueTemplate.Replace("{{assembly}}", assembly)
                .Replace("{{exchange}}", exchange)
                .Replace("{{message}}", message);

            return ApplySnakeCasing(queue);
        }
        private string ApplySnakeCasing(string value) => _snakeCase ? value.ToSnakeCase() : value;
        private MessageAttribute GeAttribute(MemberInfo type) => type.GetCustomAttribute<MessageAttribute>();
    }
}