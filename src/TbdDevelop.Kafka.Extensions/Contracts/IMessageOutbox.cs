using TbdDevelop.Kafka.Abstractions;

namespace TbdDevelop.Kafka.Extensions.Contracts;

public interface IMessageOutbox
{
    Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    Task<IEvent?> DequeueAsync(CancellationToken cancellationToken = default);
}