﻿using TbdDevelop.Kafka.Abstractions;

namespace TbdDevelop.Kafka.Outbox.Contracts;

public interface IMessageOutbox
{
    Task PostAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    Task<IOutboxMessage?> RetrieveNextMessage(CancellationToken cancellationToken = default);

    Task Commit(IOutboxMessage message, CancellationToken cancellationToken = default);
}