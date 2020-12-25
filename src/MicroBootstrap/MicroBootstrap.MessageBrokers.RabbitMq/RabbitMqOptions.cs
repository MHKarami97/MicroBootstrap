using System.Collections.Generic;
using RawRabbit.Configuration;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    public class RabbitMqOptions : RawRabbitConfiguration
    {
        public int Retries { get; set; }
        public int RetryInterval { get; set; }
        public MessageProcessorOptions MessageProcessor { get; set; }
        public IEnumerable<string> HostNames { get; set; }
        public string ConventionsCasing { get; set; }
        public new QueueOptions Queue { get; set; }
        public new ExchangeOptions Exchange { get; set; }
        public string SpanContextHeader { get; set; }
        public ContextOptions Context { get; set; }
        public LoggerOptions Logger { get; set; }

        public class LoggerOptions
        {
            public bool Enabled { get; set; }
            public string Level { get; set; }
        }
        public class ContextOptions
        {
            public bool Enabled { get; set; }
            public string Header { get; set; }
        }
        public class MessageProcessorOptions
        {
            public bool Enabled { get; set; }
            public string Type { get; set; }
            public int MessageExpirySeconds { get; set; }
        }

        public class QueueOptions : GeneralQueueConfiguration
        {
            public string Template { get; set; }
            public bool Declare { get; set; }
        }

        public class ExchangeOptions : GeneralExchangeConfiguration
        {
            public string Name { get; set; }
            public bool Declare { get; set; }
        }

        public string GetSpanContextHeader()
           => string.IsNullOrWhiteSpace(SpanContextHeader) ? "span_context" : SpanContextHeader;

        public string GetContextHeader()
        => string.IsNullOrWhiteSpace(Context?.Header) ? "message_context" : Context?.Header;
    }
}