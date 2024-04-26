using System.Collections.Concurrent;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class InMemoryMessageOutbox : IMessageOutbox
{
    private readonly ConcurrentDictionary<Guid, IOutboxMessage> _outbox = new();

    public async Task PostAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() =>
        {
            var message = new OutboxMessage<TEvent>(Guid.NewGuid(), DateTime.UtcNow, @event);

            _outbox.TryAdd(message.Identifier, message);
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
        return Task.Run(() => { _outbox.TryRemove(message.Identifier, out _); }, cancellationToken);
    }
}