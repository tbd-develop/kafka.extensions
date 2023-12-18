using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Publishing;

namespace TbdDevelop.Kafka.Outbox;

public class OutboxService(
    IMessageOutbox outbox,
    KafkaPublisher publisher,
    ILogger<OutboxService> logger) : BackgroundService
{
    private const int DefaultDelayTimeMs = 25;
    private const int BackoffIntervalMs = 1000;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            int delayTimeMs = DefaultDelayTimeMs;

            do
            {
                try
                {
                    var message = await outbox.DequeueAsync(stoppingToken);

                    if (message is null)
                        continue;

                    await publisher.PublishAsync(message, stoppingToken);

                    delayTimeMs = DefaultDelayTimeMs;
                }
                catch (Exception exception)
                {
                    delayTimeMs += BackoffIntervalMs;

                    logger.LogError(exception, "Error while publishing message. Backing off for {BackoffIntervalMs}ms",
                        BackoffIntervalMs);
                }

                await Task.Delay(delayTimeMs, stoppingToken);
            } while (!stoppingToken.IsCancellationRequested);
        }, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent);
    }
}