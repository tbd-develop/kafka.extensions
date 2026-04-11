using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Extensions.Consumption;

public class DispatchingKafkaConsumer(
    ILogger<DispatchingKafkaConsumer> logger,
    IEnumerable<ITopicConsumer> consumers) : IEventConsumer
{
    private const int TimeoutSeconds = 5;
    private const int Backoff = 3;

    private readonly IDictionary<string, int> _retryCounter = new ConcurrentDictionary<string, int>();

    public async Task BeginConsumeAsync(CancellationToken cancellationToken = default)
    {
        var tasks = consumers
            .Select(consumer =>
                Task.Factory.StartNew(
                    () => RunWithRetry(consumer, cancellationToken),
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default))
            .ToList();

        await Task.WhenAll(tasks);
    }

    private async Task RunWithRetry(ITopicConsumer consumer, CancellationToken cancellationToken)
    {
        _retryCounter.TryAdd(consumer.Topic, 0);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await consumer.Consume(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                if (_retryCounter[consumer.Topic] >= Backoff)
                {
                    return;
                }

                var restartTime = TimeoutSeconds * (_retryCounter[consumer.Topic] + 1);

                logger.LogCritical(ex, "Consumer for {Topic} failed, restarting in {RestartTime}s.", consumer.Topic,
                    restartTime);

                await Task.Delay(TimeSpan.FromSeconds(restartTime), cancellationToken);

                _retryCounter[consumer.Topic]++;
            }
        }
    }
}