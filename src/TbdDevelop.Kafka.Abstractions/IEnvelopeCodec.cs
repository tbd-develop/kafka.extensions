namespace TbdDevelop.Kafka.Abstractions;

public interface IEnvelopeCodec
{
    Type GetPayloadType(
        Type messageType
    );

    bool TryUnwrap(
        object message,
        out object payload,
        out IDictionary<string, byte[]> headers
    );

    object Wrap(
        object payload,
        IReadOnlyDictionary<string, byte[]> headers
    );
}