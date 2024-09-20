using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxService(
    IMessageOutbox outbox,
    KafkaPublisher publisher,
    ILogger<OutboxService> logger,
    IOptions<OutboxPublishingConfiguration> options) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configuration = options.Value;

        var delayTime = configuration.Interval;

        return Task.Factory.StartNew(async () =>
        {
            do
            {
                try
                {
                    var message = await outbox.RetrieveNextMessage(stoppingToken);

                    if (message is not null)
                    {
                        if (message.Event is not null)
                        {
                            await PublishMessage(message, stoppingToken);
                        }
                        else
                        {
                            await PublishDeleteMessage(message, stoppingToken);
                        }

                        await outbox.Commit(message, stoppingToken);

                        continue;
                    }

                    delayTime = configuration.Interval;
                }
                catch (Exception exception)
                {
                    if (delayTime < configuration.MaximumBackOff)
                    {
                        delayTime += configuration.BackOffOnException;
                    }

                    logger.LogError(exception, "Error while publishing message. Backing off for {BackoffIntervalMs}ms",
                        delayTime);
                }

                await Task.Delay(delayTime, stoppingToken);
            } while (!stoppingToken.IsCancellationRequested);
        }, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent);
    }

    private async Task PublishMessage(IOutboxMessage message, CancellationToken cancellationToken)
    {
        var method =
            typeof(OutboxService).GetMethod(nameof(PublishEvent), BindingFlags.NonPublic | BindingFlags.Instance);

        if (method is null)
        {
            throw new Exception("Unable to publish message");
        }

        var genericMethod = method.MakeGenericMethod(message.EventType);

        await (Task)genericMethod.Invoke(this, [message.Key, message.Event, message.Topic, cancellationToken])!;
    }

    private async Task PublishDeleteMessage(IOutboxMessage message, CancellationToken cancellationToken)
    {
        var method =
            typeof(OutboxService).GetMethod(nameof(PublishDelete), BindingFlags.NonPublic | BindingFlags.Instance);

        if (method is null)
        {
            throw new Exception("Unable to publish message");
        }

        var genericMethod = method.MakeGenericMethod(message.EventType);

        await (Task)genericMethod.Invoke(this, [message.Key, message.Topic, cancellationToken])!;
    }

    private async Task PublishEvent<TEvent>(Guid key, TEvent @event, string? topic, CancellationToken cancellationToken)
        where TEvent : class, IEvent
    {
        if (topic is not null)
        {
            await publisher.PublishAsync(key, @event, topic, cancellationToken);
        }
        else
        {
            await publisher.PublishAsync(key, @event, cancellationToken);
        }
    }

    private async Task PublishDelete<TEvent>(Guid key, string? topic, CancellationToken cancellationToken)
        where TEvent : class, IEvent
    {
        if (topic is not null)
        {
            await publisher.PublishDeleteAsync<TEvent>(key, topic, cancellationToken);
        }
        else
        {
            await publisher.PublishDeleteAsync<TEvent>(key, cancellationToken);
        }
    }
}