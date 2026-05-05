using System.Text.Json;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public static class TypeExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static byte[] Serialize<TEvent>(this TEvent data)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, Options);
    }
}