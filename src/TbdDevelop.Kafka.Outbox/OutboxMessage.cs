using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxMessage<TEvent>(Guid key, DateTime dateAdded, TEvent @event, string? topic = null) : IOutboxMessage
    where TEvent : class, IEvent
{
    public Guid Key { get; } = key;
    public DateTime AddedOn { get; } = dateAdded;
    public Type EventType { get; } = typeof(TEvent);
    public TEvent? Event { get; } = @event;

    public string? Topic { get; } = topic;

    object IOutboxMessage.Event => Event;
}