using Confluent.Kafka;

namespace TbdDevelop.Kafka.Extensions.Serializers;

public class GuidKeySerializer : ISerializer<Guid>
{
    public byte[] Serialize(Guid data, SerializationContext context)
    {
        return data.ToByteArray();
    }
}