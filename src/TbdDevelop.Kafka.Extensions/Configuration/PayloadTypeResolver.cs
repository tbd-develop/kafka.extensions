using TbdDevelop.Kafka.Abstractions;

namespace TbdDevelop.Kafka.Extensions.Configuration;

public class PayloadTypeResolver(IDictionary<string, Type> payloads)
    : IPayloadTypeResolver
{
    public bool TryResolve(
        string eventName,
        out Type payloadType
    )
    {
        return payloads.TryGetValue(eventName, out payloadType!);
    }
}