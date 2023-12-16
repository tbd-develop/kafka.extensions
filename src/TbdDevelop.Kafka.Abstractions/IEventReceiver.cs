namespace TbdDevelop.Kafka.Abstractions;

public interface IEventReceiver<in TEvent>
    where TEvent : IEvent
{
    Task ReceiveAsync(TEvent @event, CancellationToken cancellationToken = default);
}