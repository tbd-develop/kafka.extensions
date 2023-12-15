namespace TbdDevelop.Kafka.Extensions.Contracts;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}