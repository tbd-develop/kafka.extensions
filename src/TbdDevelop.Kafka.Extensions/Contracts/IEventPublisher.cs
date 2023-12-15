namespace TbdDevelop.Kafka.Extensions.Contracts;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
}