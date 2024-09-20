namespace TbdDevelop.Kafka.Outbox.Contracts;

public interface IOutboxMessage
{
    Guid Key { get; }
    DateTime AddedOn { get; }
    public Type EventType { get; }
    public object? Event { get; }
    public string? Topic { get; }
}