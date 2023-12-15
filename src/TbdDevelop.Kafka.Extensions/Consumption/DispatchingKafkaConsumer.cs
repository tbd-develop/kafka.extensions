using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Extensions.Consumption;

public class DispatchingKafkaConsumer : IEventConsumer
{
    private readonly IEnumerable<TopicConsumer> _consumers;

    public DispatchingKafkaConsumer(IEnumerable<TopicConsumer> consumers)
    {
        _consumers = consumers;
    }

    public async Task BeginConsumeAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _consumers
            .Select(consumer =>
                Task.Run(async () => await consumer.Consume(cancellationToken), cancellationToken)).ToList();

        await Task.WhenAll(tasks);
    }
}