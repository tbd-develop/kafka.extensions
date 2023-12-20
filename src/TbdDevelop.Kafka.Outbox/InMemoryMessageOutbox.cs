using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class InMemoryMessageOutbox : IMessageOutbox
{
    private readonly ILogger<InMemoryMessageOutbox> _logger;
    public InMemoryMessageOutbox(ILogger<InMemoryMessageOutbox> logger)
    {
        _logger = logger;
    }
    private readonly ConcurrentDictionary<Guid, IOutboxMessage> _outbox = new();

    public async Task PostAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() =>
        {
            var message = new OutboxMessage<TEvent>(@event);

            if (IsIdempotent(message.Identifier))
            {
                var storedMessage = GetMessageForKey(message.Identifier);
                //Console.WriteLine($"Operation with key '{message.Identifier}' and message '{storedMessage}' is already processed. Skipping.");
                _logger.LogInformation($"Operation with key '{message.Identifier}' and message '{storedMessage}' is already processed. Skipping.");
                return;
            }

            _outbox.TryAdd(message.Identifier, message);
        }, cancellationToken);
    }

    private bool IsIdempotent(Guid key)
    {
        return _outbox.ContainsKey(key);
    }

    private IOutboxMessage GetMessageForKey(Guid key)
    {
        _outbox.TryGetValue(key, out var storedMessage);
        return storedMessage; // possible null reference return
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