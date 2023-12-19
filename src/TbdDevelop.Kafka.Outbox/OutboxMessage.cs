using TbdDevelop.Kafka.Abstractions;

namespace TbdDevelop.Kafka.Outbox;

public interface IOutboxMessage
{
    Guid Identifier { get; }
    DateTime AddedOn { get; }
    public object Event { get; }
}

public class OutboxMessage<TEvent>(TEvent @event) : IOutboxMessage
    where TEvent : IEvent
{
    public Guid Identifier { get; } = Guid.NewGuid();
    public DateTime AddedOn { get; } = DateTime.UtcNow;
    public TEvent Event { get; } = @event;

    object IOutboxMessage.Event => Event;
}