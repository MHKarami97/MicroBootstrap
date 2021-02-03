using System;
using RawRabbit.Common;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions
{
    public class ConventionsProvider : IConventionsProvider
    {
        private readonly INamingConventions _conventions;

        public ConventionsProvider(INamingConventions conventions)
        {
            _conventions = conventions;
        }

        public IConventions Get<T>() where T : class
        {
            return new Conventions(typeof(T), _conventions.RoutingKeyConvention(typeof(T)),
                _conventions.ExchangeNamingConvention(typeof(T)), _conventions.QueueNamingConvention(typeof(T)));
        }
    }
}