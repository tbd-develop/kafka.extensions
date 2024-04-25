namespace TbdDevelop.Kafka.Outbox.Contracts;

public interface IOutboxMessage
{
    Guid Identifier { get; }
    DateTime AddedOn { get; }
    public object Event { get; }
}