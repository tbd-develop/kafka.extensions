using System.Collections.Concurrent;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class InMemoryMessageOutbox : IMessageOutbox
{
    private readonly ConcurrentDictionary<Guid, IOutboxMessage> _outbox = new();

    public async Task PostAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() =>
        {
            var message = new InMemoryOutboxMessage<TEvent>(Guid.NewGuid(), key, DateTime.UtcNow, @event);

            _outbox.TryAdd(message.Id, message);
        }, cancellationToken);
    }

    public Task<IOutboxMessage?> RetrieveNextMessage(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var nextMessage = (from message in _outbox.Values
                orderby message.AddedOn
                select message).FirstOrDefault();

            return nextMessage;
        }, cancellationToken);
    }

    public Task Commit(IOutboxMessage message, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (message is not IInMemoryOutboxMessage inMemoryMessage)
                return;

            _outbox.TryRemove(inMemoryMessage.Id, out _);
        }, cancellationToken);
    }

    private class InMemoryOutboxMessage<TEvent>(Guid id, Guid key, DateTime dateAdded, TEvent @event)
        : OutboxMessage<TEvent>(key, dateAdded, @event), IInMemoryOutboxMessage
        where TEvent : IEvent
    {
        public Guid Id { get; } = id;
    }

    private interface IInMemoryOutboxMessage : IOutboxMessage
    {
        Guid Id { get; }
    }
}