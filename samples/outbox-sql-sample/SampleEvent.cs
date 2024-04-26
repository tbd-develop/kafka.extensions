using TbdDevelop.Kafka.Abstractions;

namespace outbox_sql_sample;

public class SampleEvent : IEvent
{
    public string Name { get; set; } = null!;
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
}