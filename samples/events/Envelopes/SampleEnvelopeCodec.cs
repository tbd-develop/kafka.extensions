using TbdDevelop.Kafka.Abstractions;

namespace events.Envelopes;

public class SampleEnvelopeCodec : EnvelopeCodecBase
{
    public override Type GetPayloadType(
        Type messageType
    )
        => messageType.IsGenericType && messageType.GetGenericTypeDefinition() ==
            typeof(SampleEnvelope<>)
                ? messageType.GetGenericArguments()[0]
                : messageType;

    protected override Type EnvelopeOpenType => typeof(SampleEnvelope<>);

    protected override void TryUnwrapImpl(
        dynamic envelope,
        ref IDictionary<string, byte[]> headers
    )
    {
        headers.Add("category", Utf8((string)envelope.Category));
    }

    protected override void WrapImpl(
        ref object envelope,
        IReadOnlyDictionary<string, byte[]> headers
    )
    {
        Set(envelope, "Category", Str(headers["category"]));
    }
}