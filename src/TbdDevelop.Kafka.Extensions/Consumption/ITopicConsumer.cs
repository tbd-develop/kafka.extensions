namespace TbdDevelop.Kafka.Extensions.Consumption;

public interface ITopicConsumer
{
    string Topic { get; }
    Task Consume(CancellationToken cancellationToken);
}