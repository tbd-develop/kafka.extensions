using System.Text.Json;
using Confluent.Kafka;

namespace TbdDevelop.Kafka.Extensions.Serializers;

public class EventSerializer<TEvent> : ISerializer<TEvent>
    where TEvent : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public byte[] Serialize(TEvent data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, Options);
    }
}