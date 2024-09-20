namespace TbdDevelop.Kafka.Abstractions;

public interface IEventPublisher
{
    Task PublishDeleteAsync<TEvent>(Guid key, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    Task PublishDeleteAsync<TEvent>(Guid key, string topic, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    Task PublishAsync<TEvent>(Guid key, TEvent @event, string topic, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    Task PublishAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
}