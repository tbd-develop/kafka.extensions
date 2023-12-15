using Confluent.Kafka;

namespace TbdDevelop.Kafka.Extensions.Deserializers;

public class GuidKeyDeserializer : IDeserializer<Guid>
{
    public Guid Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return new Guid(data);
    }
}