﻿using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxService(
    IMessageOutbox outbox,
    KafkaPublisher publisher,
    ILogger<OutboxService> logger) : BackgroundService
{
    private const int DefaultDelayTimeMs = 25;
    private const int BackoffIntervalMs = 1000;
    private const int MaxBackOffIntervalMs = 10000;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayTimeMs = DefaultDelayTimeMs;

        return Task.Factory.StartNew(async () =>
        {
            do
            {
                try
                {
                    var message = await outbox.RetrieveNextMessage(stoppingToken);

                    if (message is null)
                        continue;

                    await PublishMessage(message, stoppingToken);

                    await outbox.Commit(message, stoppingToken);

                    delayTimeMs = DefaultDelayTimeMs;
                }
                catch (Exception exception)
                {
                    if (delayTimeMs < MaxBackOffIntervalMs)
                    {
                        delayTimeMs += BackoffIntervalMs;
                    }

                    logger.LogError(exception, "Error while publishing message. Backing off for {BackoffIntervalMs}ms",
                        BackoffIntervalMs);
                }

                await Task.Delay(delayTimeMs, stoppingToken);
            } while (!stoppingToken.IsCancellationRequested);
        }, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent);
    }

    private async Task PublishMessage(IOutboxMessage message, CancellationToken cancellationToken)
    {
        var type = message.Event.GetType();

        var method =
            typeof(OutboxService).GetMethod(nameof(PublishEvent), BindingFlags.NonPublic | BindingFlags.Instance);

        if (method is null)
        {
            throw new Exception("Unable to publish message");
        }

        var genericMethod = method.MakeGenericMethod(type);

        await (Task)genericMethod.Invoke(this, [message.Key, message.Event, cancellationToken])!;
    }

    private async Task PublishEvent<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken)
        where TEvent : class, IEvent
    {
        await publisher.PublishAsync(key, @event, cancellationToken);
    }
}