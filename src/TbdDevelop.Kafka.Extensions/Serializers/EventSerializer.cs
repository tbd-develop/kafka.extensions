using System.Text.Json;
using Confluent.Kafka;

namespace TbdDevelop.Kafka.Extensions.Serializers;

public class EventSerializer<TEvent> : ISerializer<TEvent>
    where TEvent : class
{
    public byte[] Serialize(TEvent data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}