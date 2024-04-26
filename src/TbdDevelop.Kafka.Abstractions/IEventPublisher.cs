namespace TbdDevelop.Kafka.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
}