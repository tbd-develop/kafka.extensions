using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxPublisher(IMessageOutbox outbox) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return outbox.EnqueueAsync(@event, cancellationToken);
    }
}