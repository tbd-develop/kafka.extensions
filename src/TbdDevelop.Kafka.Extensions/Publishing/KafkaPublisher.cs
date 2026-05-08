using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Instrumentation;

namespace TbdDevelop.Kafka.Extensions.Publishing;

public class KafkaPublisher(
    ILogger<KafkaPublisher> logger,
    KafkaConfiguration configuration,
    IProducer<Guid, byte[]> producer,
    IEnvelopeCodec? codec = null)
    : IEventPublisher, IAsyncDisposable
{
    private readonly ILogger _logger = logger;
    private static readonly ActivitySource ActivitySource = new(KafkaInstrumentation.PublishingSourceName, "0.0.1");

    public async Task PublishAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            var (body, headers) = FetchBodyFromEnvelope(@event);

            if (!configuration.TryGetTopicFromEventType(body.GetType(), out string? topic))
            {
                _logger.LogCritical("No topic found for event type {EventType}", typeof(TEvent).Name);

                return;
            }

            await PublishInternalAsync(key, body, headers, topic!, cancellationToken);
        }
        catch (ArgumentNullException exception)
        {
            _logger.LogCritical(exception, "Configuration does not provide topic for {EventType}",
                typeof(TEvent).Name);
        }
    }

    public async Task PublishAsync<TEvent>(Guid key, TEvent @event, string topic,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        var (body, headers) = FetchBodyFromEnvelope(@event);

        await PublishInternalAsync(key, body, headers, topic!, cancellationToken);
    }

    public async Task PublishDeleteAsync<TEvent>(Guid key, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        if (!configuration.TryGetTopicFromEventType<TEvent>(out string? topic))
        {
            _logger.LogCritical("No topic found for event type {EventType}", typeof(TEvent).Name);

            return;
        }

        await PublishDeleteAsync<TEvent>(key, topic!, cancellationToken);
    }

    public async Task PublishDeleteAsync<TEvent>(Guid key, string topic, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await producer.ProduceAsync(topic,
            new Message<Guid, byte[]>
            {
                Key = key,
                Timestamp = new Timestamp(DateTime.UtcNow)
            }, cancellationToken);
    }

    private async Task PublishInternalAsync(
        Guid key,
        object @event,
        IDictionary<string, byte[]>? headers,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var message = ConstructMessage(key, @event, topic, headers);

            await producer.ProduceAsync(topic, message, cancellationToken);

            KafkaMetrics.MessagesPublished.Add(1, new TagList { { "topic", topic } });
        }
        catch
        {
            KafkaMetrics.PublishFailures.Add(1, new TagList { { "topic", topic } });
        }
        finally
        {
            KafkaMetrics.PublishDuration.Record(sw.Elapsed.TotalMilliseconds, new TagList { { "topic", topic } });
        }
    }

    private static Message<Guid, byte[]> ConstructMessage(Guid key,
        object @event,
        string topic,
        IDictionary<string, byte[]>? headers)
    {
        using var activity = ActivitySource.StartActivity($"kafka.publish {topic}", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", topic);
        activity?.SetTag("messaging.destination_kind", "topic");
        activity?.SetTag("messaging.message_id", key.ToString());
        activity?.SetTag("messaging.kafka.message_key", key.ToString());

        var message = new Message<Guid, byte[]>
        {
            Key = key,
            Timestamp = ResolveTimestamp(@event, headers),
            Value = @event.Serialize()
        };

        if (headers is not { Count: > 0 })
        {
            return message;
        }

        message.Headers = [];

        foreach (var (name, value) in headers)
        {
            message.Headers.Add(name, value);
        }

        if (activity is not null)
        {
            DistributedContextPropagator.Current.Inject(activity, headers, (carrier, k, v) =>
                ((IDictionary<string, byte[]>)carrier!)[k] = Encoding.UTF8.GetBytes(v));
        }

        return message;
    }

    public async ValueTask DisposeAsync()
    {
        producer.Flush(TimeSpan.FromSeconds(10));
        producer.Dispose();

        await Task.CompletedTask;
    }

    private static Timestamp ResolveTimestamp(object body, IDictionary<string, byte[]>? headers)
    {
        if (headers is not null && headers.TryGetValue("occurred-at", out var raw))
        {
            return new Timestamp(DateTimeOffset.Parse(Encoding.UTF8.GetString(raw)).UtcDateTime);
        }

        if (body is IEvent e)
        {
            return new Timestamp(e.OccurredOn);
        }

        return new Timestamp(DateTime.UtcNow);
    }

    private (object body, IDictionary<string, byte[]>? headers) FetchBodyFromEnvelope<TEvent>(TEvent @event)
        where TEvent : class
    {
        object body;
        IDictionary<string, byte[]>? headers = null;

        if (codec is not null && codec.TryUnwrap(@event, out var payload, out var hdrs))
        {
            body = payload;
            headers = hdrs;
        }
        else
        {
            body = @event;
        }

        return (body, headers);
    }
}