using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxMessage<TEvent>(Guid identifier, DateTime dateAdded, TEvent @event) : IOutboxMessage
    where TEvent : IEvent
{
    public Guid Identifier { get; } = identifier;
    public DateTime AddedOn { get; } = dateAdded;
    public TEvent Event { get; } = @event;

    object IOutboxMessage.Event => Event;
}