﻿using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxPublisher(IMessageOutbox outbox) : IEventPublisher
{
    public Task PublishDeleteAsync<TEvent>(Guid key, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return outbox.PostAsync<TEvent>(key, cancellationToken);
    }

    public Task PublishDeleteAsync<TEvent>(Guid key, string topic, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return outbox.PostAsync<TEvent>(key, topic, cancellationToken);
    }

    public Task PublishAsync<TEvent>(Guid key, TEvent @event, string topic,
        CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        return outbox.PostAsync(key, @event, topic, cancellationToken);
    }

    public Task PublishAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return outbox.PostAsync(key, @event, cancellationToken);
    }
}