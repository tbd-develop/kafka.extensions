namespace TbdDevelop.Kafka.Abstractions;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}