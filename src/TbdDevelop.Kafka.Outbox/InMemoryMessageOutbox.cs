using System.Collections.Concurrent;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class InMemoryMessageOutbox : IMessageOutbox
{
    private readonly ConcurrentQueue<IEvent> _queue = new();

    public async Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() => { _queue.Enqueue(@event); }, cancellationToken);
    }

    public async Task<IEvent?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _queue.TryDequeue(out var @event) ? @event : null, cancellationToken);
    }
}