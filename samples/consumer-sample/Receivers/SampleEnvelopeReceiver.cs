using System.Text.Json;
using events.Envelopes;
using TbdDevelop.Kafka.Abstractions;

namespace consumer_sample.Receivers;

public class SampleEnvelopeReceiver : EventReceiver<SampleEnvelope<JsonElement>>
{
    public override Task ReceiveAsync(SampleEnvelope<JsonElement> @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{@event.Category}");

        return Task.CompletedTask;
    }
}