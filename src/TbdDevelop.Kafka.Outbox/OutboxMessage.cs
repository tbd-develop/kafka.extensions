using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxMessage<TEvent>(TEvent @event) : IOutboxMessage
    where TEvent : IEvent
{
    public Guid Identifier { get; } = Guid.NewGuid();
    public DateTime AddedOn { get; } = DateTime.UtcNow;
    public TEvent Event { get; } = @event;

    object IOutboxMessage.Event => Event;
}