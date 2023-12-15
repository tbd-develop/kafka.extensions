using System.Text.Json;
using Confluent.Kafka;

namespace TbdDevelop.Kafka.Extensions.Deserializers;

public class EventDeserializer<TEntity> : IDeserializer<TEntity>
    where TEntity : class
{
    public TEntity Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<TEntity>(data)!;
    }
}