﻿using System.Collections.Concurrent;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class InMemoryMessageOutbox : IMessageOutbox
{
    private readonly ConcurrentDictionary<Guid, IOutboxMessage> _outbox = new();

    public async Task PostAsync<TEvent>(Guid key, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() =>
        {
            var message = new InMemoryOutboxMessage<TEvent>(Guid.NewGuid(), key, DateTime.UtcNow);

            _outbox.TryAdd(message.Id, message);
        });
    }

    public async Task PostAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() =>
        {
            var message = new InMemoryOutboxMessage<TEvent>(Guid.NewGuid(), key, DateTime.UtcNow, @event);

            _outbox.TryAdd(message.Id, message);
        }, cancellationToken);
    }

    public async Task PostAsync<TEvent>(Guid key, string topic, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() =>
        {
            var message = new InMemoryOutboxMessage<TEvent>(Guid.NewGuid(), key, DateTime.UtcNow, null, topic);

            _outbox.TryAdd(message.Id, message);
        });
    }

    public async Task PostAsync<TEvent>(Guid key, TEvent @event, string topic,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await Task.Run(() =>
        {
            var message = new InMemoryOutboxMessage<TEvent>(Guid.NewGuid(), key, DateTime.UtcNow, @event, topic);

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

    private class InMemoryOutboxMessage<TEvent>(
        Guid id,
        Guid key,
        DateTime dateAdded,
        TEvent? @event = null,
        string? topic = null)
        : OutboxMessage<TEvent>(key, dateAdded, @event, topic), IInMemoryOutboxMessage
        where TEvent : class, IEvent
    {
        public Guid Id { get; } = id;
    }

    private interface IInMemoryOutboxMessage : IOutboxMessage
    {
        Guid Id { get; }
    }
}