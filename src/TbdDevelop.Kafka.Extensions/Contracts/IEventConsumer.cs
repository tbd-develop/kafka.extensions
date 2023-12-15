namespace TbdDevelop.Kafka.Extensions.Contracts;

public interface IEventConsumer
{
    Task BeginConsumeAsync(CancellationToken cancellationToken = default);
}