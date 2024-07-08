using events;
using TbdDevelop.Kafka.Abstractions;

namespace consumer_sample.Handlers;

public class SampleEventReceiver : EventReceiver<SampleEvent>
{
    public override Task ReceiveAsync(SampleEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{@event.SomeValue} {@event.SomeOtherValue}");

        return Task.CompletedTask;
    }
}