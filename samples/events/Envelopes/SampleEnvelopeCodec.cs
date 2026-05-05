using System.Runtime.CompilerServices;
using System.Text;
using TbdDevelop.Kafka.Abstractions;

namespace events.Envelopes;

public class SampleEnvelopeCodec : IEnvelopeCodec
{
    public Type GetPayloadType(Type messageType)
        => messageType.IsGenericType && messageType.GetGenericTypeDefinition() ==
            typeof(SampleEnvelope<>)
                ? messageType.GetGenericArguments()[0]
                : messageType;

    public bool TryUnwrap(object message, out object payload, out IDictionary<string, byte[]> headers)
    {
        var t = message.GetType();

        if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(SampleEnvelope<>))
        {
            payload = null!;
            headers = null!;
            return false;
        }

        dynamic env = message;
        payload = env.Payload;

        headers = new Dictionary<string, byte[]>
        {
            ["category"] = Utf8(env.Category),
        };
        return true;
    }

    public object Wrap(object payload, IReadOnlyDictionary<string, byte[]> headers)
    {
        var envelopeType = typeof(SampleEnvelope<>).MakeGenericType(payload.GetType());
        var envelope = RuntimeHelpers.GetUninitializedObject(envelopeType);

        Set(envelope, "Category", Str(headers["category"]));
        Set(envelope, "Payload", payload);
        
        return envelope;
    }

    private static void Set(object obj, string name, object value)
        => obj.GetType().GetProperty(name)!.SetValue(obj, value);

    private static string Str(byte[] b) => Encoding.UTF8.GetString(b);

    private static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s);
}