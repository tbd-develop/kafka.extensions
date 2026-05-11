using System.Text.Json;
using events;
using events.Envelopes;
using TbdDevelop.Kafka.Abstractions;

namespace consumer_sample.Receivers;

public class SampleMultipleEventReceiver : MultiEventReceiver,
    IReceive<SampleEnvelope<SampleEvent>>
{
    public Task ReceiveAsync(SampleEnvelope<SampleEvent> @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(@event.Category);
        Console.WriteLine(@event.Payload.SomeOtherValue);

        return Task.CompletedTask;
    }
}