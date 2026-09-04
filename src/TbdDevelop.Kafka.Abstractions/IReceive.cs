namespace TbdDevelop.Kafka.Abstractions;

public interface IReceive<in TEvent>
    where TEvent : class
{
    Task ReceiveAsync(
        TEvent @event,
        CancellationToken cancellationToken = default
    );
}