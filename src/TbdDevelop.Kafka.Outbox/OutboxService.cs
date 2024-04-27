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

                    if (message is null)
                        continue;

                    await PublishMessage(message, stoppingToken);

                    await outbox.Commit(message, stoppingToken);

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