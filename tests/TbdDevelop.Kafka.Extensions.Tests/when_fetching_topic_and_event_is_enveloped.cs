using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Extensions.Tests.Messages;
using Xunit;

namespace TbdDevelop.Kafka.Extensions.Tests;

public class when_fetching_topic_and_event_is_enveloped
{
    private readonly ILogger<KafkaPublisher> _logger = Substitute.For<ILogger<KafkaPublisher>>();
    private readonly IEnvelopeCodec _codec = Substitute.For<IEnvelopeCodec>();
    private readonly IProducer<Guid, byte[]> _producer = Substitute.For<IProducer<Guid, byte[]>>();
    private KafkaConfiguration _configuration = null!;
    private KafkaPublisher _subject = null!;

    private readonly Guid _identifier = Guid.NewGuid();
    private const string _envelopeName = "envelope-name";
    private const string _eventTitle = "event-title";
    private const int _eventAge = 42;
    private const string _topicName = "topic.sample.event";

    private IDictionary<string, byte[]> _expectedHeaders = null!;

    private readonly SampleEnvelope<SampleEvent> _event = new()
    {
        Name = _envelopeName,
        Payload = new SampleEvent
        {
            Age = _eventAge,
            Title = _eventTitle
        }
    };

    [Fact]
    public async Task try_get_topic_is_called_with_event_type()
    {
        _expectedHeaders = new Dictionary<string, byte[]>
        {
            { "name", Encoding.UTF8.GetBytes(_envelopeName) }
        };

        _configuration = new KafkaConfiguration()
        {
            Producer = new Dictionary<string, string>() { },
            Topics = new List<TopicConfiguration>
            {
                new() { Name = _topicName, TypeName = typeof(SampleEvent).AssemblyQualifiedName! }
            }
        };

        _subject = new KafkaPublisher(
            _logger,
            _configuration,
            _producer,
            _codec);

        _codec.TryUnwrap(
            Arg.Is<SampleEnvelope<SampleEvent>>(s => s.Name == _envelopeName),
            out Arg.Any<object>(),
            out Arg.Any<IDictionary<string, byte[]>>()
        ).Returns(call =>
        {
            call[1] = _event.Payload;
            call[2] = _expectedHeaders;

            return true;
        });

        await _subject.PublishAsync(
            _identifier, _event, CancellationToken.None
        );

        await _producer
            .Received()
            .ProduceAsync(Arg.Is<string>(_topicName),
                Arg.Is<Message<Guid, byte[]>>(m => m.Key == _identifier),
                Arg.Any<CancellationToken>());
    }
}