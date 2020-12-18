using System;
using MicroBootstrap.Types;

namespace MicroBootstrap.MessageBrokers.Outbox.Messages
{
    public sealed class InboxMessage : IIdentifiable<string>
    {
        public string Id { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}