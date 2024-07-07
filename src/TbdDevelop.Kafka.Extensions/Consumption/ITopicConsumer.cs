namespace TbdDevelop.Kafka.Extensions.Consumption;

public interface ITopicConsumer
{
    Task Consume(CancellationToken cancellationToken);
}