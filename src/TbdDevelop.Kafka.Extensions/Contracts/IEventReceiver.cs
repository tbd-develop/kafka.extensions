namespace TbdDevelop.Kafka.Extensions.Contracts;

public interface IEventReceiver<in TEvent>
    where TEvent : IEvent
{
    Task ReceiveAsync(TEvent @event, CancellationToken cancellationToken = default);
}