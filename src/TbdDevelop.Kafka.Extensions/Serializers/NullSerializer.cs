using Confluent.Kafka;

namespace TbdDevelop.Kafka.Extensions.Serializers;

public class NullSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return null!;
    }
}