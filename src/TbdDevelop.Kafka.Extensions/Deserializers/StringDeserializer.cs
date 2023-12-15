using System.Text;
using Confluent.Kafka;

namespace TbdDevelop.Kafka.Extensions.Deserializers;

public class StringDeserializer : IDeserializer<string>
{
    public string Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return Encoding.UTF8.GetString(data);
    }
}