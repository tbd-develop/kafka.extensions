using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Extensions.Consumption;

public class DispatchingKafkaConsumer(IEnumerable<TopicConsumer> consumers) : IEventConsumer
{
    public async Task BeginConsumeAsync(CancellationToken cancellationToken = default)
    {
        var tasks = consumers
            .Select(consumer =>
                Task
                    .Factory
                    .StartNew(async () => await consumer.Consume(cancellationToken)
                        , TaskCreationOptions.LongRunning)
            ).ToList();

        await Task.WhenAll(tasks);
    }
}