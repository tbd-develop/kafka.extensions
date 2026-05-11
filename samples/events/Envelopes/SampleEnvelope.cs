using TbdDevelop.Kafka.Abstractions;

namespace events.Envelopes;

public class SampleEnvelope<TPayload> : IEnvelope
{
    public required string Category { get; set; }
    public required TPayload Payload { get; set; }

    public string EventName { get; set; } = typeof(TPayload).Name;
}