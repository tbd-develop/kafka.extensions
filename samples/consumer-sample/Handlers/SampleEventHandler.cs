using events;
using TbdDevelop.Kafka.Extensions.Contracts;

namespace consumer_sample.Handlers;

public class SampleEventHandler : IEventReceiver<SampleEvent>
{
    public Task ReceiveAsync(SampleEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{@event.SomeValue} {@event.SomeOtherValue}");

        return Task.CompletedTask;
    }
}