using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxPublisher(IMessageOutbox outbox) : IEventPublisher
{
    public Task PublishAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return outbox.PostAsync(key, @event, cancellationToken);
    }
}