namespace TbdDevelop.Kafka.Extensions.Tests.Messages;

public class SampleEnvelope<TPayload>
    where TPayload : class
{
    public string Name { get; set; } = null!;
    public TPayload Payload { get; set; } = null!;
}