namespace TbdDevelop.Kafka.Abstractions;

public interface IPayloadTypeResolver
{
    bool TryResolve(
        string eventName,
        out Type payloadType
    );
}