using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Deserializers;
using TbdDevelop.Kafka.Extensions.Instrumentation;

namespace TbdDevelop.Kafka.Extensions.Consumption;

public class TopicConsumer<TEvent>
    : ITopicConsumer
    where TEvent : class
{
    private static readonly ActivitySource ActivitySource = new(KafkaInstrumentation.ConsumptionSourceName, "0.0.1");

    public string Topic { get; }

    private readonly IDictionary<string, string> _topicConfiguration;
    private readonly IEventReceiver<TEvent> _eventReceiver;
    private readonly ILogger<TopicConsumer<TEvent>> _logger;
    private readonly IEnvelopeCodec? _codec;
    private readonly bool _requiresWrap;
    private readonly Type _payloadType;

    public TopicConsumer(
        string topicToSubscribe,
        IDictionary<string, string> topicConfiguration,
        IEventReceiver<TEvent> eventReceiver,
        ILogger<TopicConsumer<TEvent>> logger,
        IEnvelopeCodec? codec = null)
    {
        Topic = topicToSubscribe;
        _topicConfiguration = topicConfiguration;
        _eventReceiver = eventReceiver;
        _logger = logger;
        _codec = codec;

        _payloadType = codec?.GetPayloadType(typeof(TEvent)) ?? typeof(TEvent);
        _requiresWrap = _payloadType != typeof(TEvent);
    }

    public async Task Consume(CancellationToken cancellationToken)
    {
        try
        {
            using var consumer = new ConsumerBuilder<Guid, string>(_topicConfiguration)
                .SetKeyDeserializer(new GuidKeyDeserializer())
                .SetValueDeserializer(new StringDeserializer())
                .SetErrorHandler((_, error) => _logger.LogError(error.Reason))
                .SetLogHandler((_, logMessage) => _logger.LogInformation(logMessage.Message))
                .Build();

            consumer.Subscribe(Topic);

            _logger.LogInformation("Starting {ConsumerName}, subscribed to {Topic}", GetType().Name, Topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = consumer.Consume(cancellationToken);

                if (result.Message is null)
                {
                    continue;
                }

                try
                {
                    if (!await HandleMessage(result, cancellationToken))
                    {
                        continue;
                    }

                    consumer.Commit(result);
                }
                catch (JsonException ex)
                {
                    _logger.LogCritical(ex, "Failed to deserialize message on {Topic}, skipping.", Topic);

                    consumer.Commit(result);
                }
            }
        }
        catch (Exception generalException)
        {
            _logger.LogCritical(generalException, "Consumer Failure for Topic {Topic}", Topic);
        }
    }

    private async Task<bool> HandleMessage(ConsumeResult<Guid, string> result, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"kafka.consume {Topic}", ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", Topic);
        activity?.SetTag("messaging.kafka.partition", result.Partition.Value);
        activity?.SetTag("messaging.kafka.offset", result.Offset.Value);

        if (string.IsNullOrEmpty(result.Message.Value))
        {
            await _eventReceiver.DeleteAsync(result.Message.Key, cancellationToken);

            return true;
        }

        var payload = JsonSerializer.Deserialize(result.Message.Value, _payloadType, DefaultJsonSerializerOptions);

        if (payload is null)
        {
            _logger.LogError("{TopicToSubscribe} / {Message} message could not be deserialized",
                Topic,
                result.Message.Value);

            return false;
        }

        TEvent @event;

        if (_requiresWrap)
        {
            var headers = (result.Message.Headers ?? new Headers())
                .ToDictionary(h => h.Key, h => h.GetValueBytes());

            @event = (TEvent)_codec!.Wrap(payload, headers);
        }
        else
        {
            @event = (TEvent)payload;
        }

        await _eventReceiver.ReceiveAsync(@event, cancellationToken);

        return true;
    }

    private static JsonSerializerOptions DefaultJsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}