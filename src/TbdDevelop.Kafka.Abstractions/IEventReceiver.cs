namespace TbdDevelop.Kafka.Abstractions;

public interface IEventReceiver
{
    Task ReceiveAsync(object @event, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid key, CancellationToken cancellationToken = default);
}

public interface IEventReceiver<in TEvent> : IEventReceiver
    where TEvent : class
{
    Task ReceiveAsync(TEvent @event, CancellationToken cancellationToken = default);
}