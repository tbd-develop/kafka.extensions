using System.Runtime.CompilerServices;
using System.Text;

namespace TbdDevelop.Kafka.Abstractions;

public abstract class EnvelopeCodecBase<TEnvelope> : IEnvelopeCodec
    where TEnvelope : IEnvelope
{
    protected abstract Type EnvelopeOpenType { get; }

    protected abstract void TryUnwrapImpl(
        dynamic envelope,
        ref IDictionary<string, byte[]> headers);

    protected abstract void WrapImpl(
        ref object envelope,
        IReadOnlyDictionary<string, byte[]> headers);

    public Type GetPayloadType(Type messageType)
        => messageType.IsGenericType && messageType.GetGenericTypeDefinition() ==
            typeof(TEnvelope)
                ? messageType.GetGenericArguments()[0]
                : messageType;

    public bool TryUnwrap(object message, out object payload, out IDictionary<string, byte[]> headers)
    {
        var t = message.GetType();

        if (!t.IsGenericType || t.GetGenericTypeDefinition() != EnvelopeOpenType)
        {
            payload = null!;
            headers = null!;

            return false;
        }

        dynamic envelope = message;
        payload = envelope.Payload;

        headers = new Dictionary<string, byte[]>
        {
            ["event-name"] = Utf8(envelope.EventName)
        };

        TryUnwrapImpl(envelope, ref headers);

        return true;
    }

    public object Wrap(object payload, IReadOnlyDictionary<string, byte[]> headers)
    {
        var eventName = Str(headers["event-name"]);

        var envelopeType = EnvelopeOpenType.MakeGenericType(payload.GetType());
        var envelope = RuntimeHelpers.GetUninitializedObject(envelopeType);

        Set(envelope, "EventName", eventName);
        Set(envelope, "Payload", payload);

        WrapImpl(ref envelope, headers);

        return envelope;
    }

    protected static void Set(object obj, string name, object value)
        => obj.GetType().GetProperty(name)!.SetValue(obj, value);

    protected static string Str(byte[] b) => Encoding.UTF8.GetString(b);

    protected static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s);
}

public class PayloadTypeException(string message) : Exception(message);