namespace TbdDevelop.Kafka.Abstractions;

public abstract class DefaultEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}