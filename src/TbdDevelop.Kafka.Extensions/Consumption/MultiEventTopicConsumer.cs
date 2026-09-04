using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Deserializers;
using TbdDevelop.Kafka.Extensions.Instrumentation;

namespace TbdDevelop.Kafka.Extensions.Consumption;

public class MultiEventTopicConsumer(
    string topicToSubscribe,
    IDictionary<string, string> topicConfiguration,
    IEventReceiver eventReceiver,
    ILogger<MultiEventTopicConsumer> logger,
    IEnvelopeCodec codec,
    IPayloadTypeResolver resolver)
    : ITopicConsumer
{
    private static readonly ActivitySource ActivitySource = new(KafkaInstrumentation.ConsumptionSourceName, "0.0.1");

    public string Topic { get; } = topicToSubscribe;

    public async Task Consume(
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var consumer = new ConsumerBuilder<Guid, string>(topicConfiguration)
                .SetKeyDeserializer(new GuidKeyDeserializer())
                .SetValueDeserializer(new StringDeserializer())
                .SetErrorHandler((_, error) => logger.LogError(error.Reason))
                .SetLogHandler((_, logMessage) => logger.LogInformation(logMessage.Message))
                .Build();

            consumer.Subscribe(Topic);

            logger.LogInformation("Starting {ConsumerName}, subscribed to {Topic}", GetType().Name, Topic);

            while ( !cancellationToken.IsCancellationRequested )
            {
                var result = consumer.Consume(cancellationToken);

                if ( result.Message is null )
                {
                    continue;
                }

                try
                {
                    if ( !await HandleMessage(result, cancellationToken) )
                    {
                        continue;
                    }

                    consumer.Commit(result);
                }
                catch ( JsonException ex )
                {
                    logger.LogCritical(ex, "Failed to deserialize message on {Topic}, skipping.", Topic);

                    consumer.Commit(result);
                }
            }
        }
        catch ( Exception generalException )
        {
            logger.LogCritical(generalException, "Consumer Failure for Topic {Topic}", Topic);
        }
    }

    private async Task<bool> HandleMessage(
        ConsumeResult<Guid, string> result,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.StartActivity($"kafka.consume {Topic}", ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", Topic);
        activity?.SetTag("messaging.kafka.partition", result.Partition.Value);
        activity?.SetTag("messaging.kafka.offset", result.Offset.Value);

        if ( string.IsNullOrEmpty(result.Message.Value) )
        {
            await eventReceiver.DeleteAsync(result.Message.Key, cancellationToken);

            return true;
        }

        var headers = (result.Message.Headers ?? new Headers())
            .ToDictionary(h => h.Key, h => h.GetValueBytes());

        var eventName = Encoding.UTF8.GetString(headers["event-name"]);

        if ( !resolver.TryResolve(eventName, out var payloadType) )
        {
            logger.LogError("{Topic} / unresolvable event {EventName}", Topic, eventName);
            return false;
        }

        var payload =
            JsonSerializer.Deserialize(result.Message.Value, payloadType, DefaultJsonSerializerOptions);

        if ( payload is null )
        {
            logger.LogError("{TopicToSubscribe} / {Message} message could not be deserialized",
                Topic,
                result.Message.Value);

            return false;
        }

        var @event = codec.Wrap(payload, headers);

        await eventReceiver.ReceiveAsync(@event, cancellationToken);

        return true;
    }

    private static JsonSerializerOptions DefaultJsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}