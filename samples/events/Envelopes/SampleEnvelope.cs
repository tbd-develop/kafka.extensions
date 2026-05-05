namespace events.Envelopes;

public class SampleEnvelope<TPayload>
{
    public required string Category { get; set; }
    public required TPayload Payload { get; set; } 
}