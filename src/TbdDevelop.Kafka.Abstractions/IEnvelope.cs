namespace TbdDevelop.Kafka.Abstractions;

public interface IEnvelope
{
    string EventName { get; set; }
}