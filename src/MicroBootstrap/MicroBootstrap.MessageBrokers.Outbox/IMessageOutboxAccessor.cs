using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers.Outbox.Messages;

namespace MicroBootstrap.MessageBrokers.Outbox
{
    public interface IMessageOutboxAccessor
    {
        Task<IReadOnlyList<OutboxMessage>> GetUnsentAsync();
        Task ProcessAsync(OutboxMessage message);
        Task ProcessAsync(IEnumerable<OutboxMessage> outboxMessages);
    }
}