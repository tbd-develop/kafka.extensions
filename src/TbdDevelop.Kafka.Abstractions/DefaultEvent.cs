namespace TbdDevelop.Kafka.Abstractions;

public abstract class DefaultEvent 
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}