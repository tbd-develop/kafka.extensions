namespace TbdDevelop.Kafka.Outbox.Contracts;

public interface IOutboxMessage
{
    Guid Key { get; }
    DateTime AddedOn { get; }
    public object Event { get; }
}