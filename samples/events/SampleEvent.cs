using TbdDevelop.Kafka.Abstractions;

namespace events;

public class SampleEvent : DefaultEvent
{
    public string SomeValue { get; set; } = null!;
    public int SomeOtherValue { get; set; }
}