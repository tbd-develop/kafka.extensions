namespace TbdDevelop.Kafka.Abstractions;

public interface IEventReceiver
{
    Task ReceiveAsync(object @event, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid key, CancellationToken cancellationToken = default);
}

public interface IEventReceiver<in TEvent> : IEventReceiver
    where TEvent : IEvent
{
    Task ReceiveAsync(TEvent @event, CancellationToken cancellationToken = default);
}

public abstract class EventReceiver<TEvent> : IEventReceiver<TEvent>
    where TEvent : IEvent
{
    public abstract Task ReceiveAsync(TEvent @event, CancellationToken cancellationToken = default);

    public virtual Task DeleteAsync(Guid key, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ReceiveAsync(object @event, CancellationToken cancellationToken = default)
    {
        if (@event is TEvent typedEvent)
        {
            return ReceiveAsync(typedEvent, cancellationToken);
        }

        throw new ArgumentException(
            $"Expected event of type {typeof(TEvent).Name}, but received {(@event?.GetType().Name ?? "null")}");
    }
}