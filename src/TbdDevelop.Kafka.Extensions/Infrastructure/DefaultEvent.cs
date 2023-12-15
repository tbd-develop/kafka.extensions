using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public abstract class DefaultEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}