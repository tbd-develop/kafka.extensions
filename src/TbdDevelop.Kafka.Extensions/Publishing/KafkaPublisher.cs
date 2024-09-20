using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Serializers;

namespace TbdDevelop.Kafka.Extensions.Publishing;

public class KafkaPublisher(ILogger<KafkaPublisher> logger, KafkaConfiguration configuration)
    : IEventPublisher
{
    private readonly ILogger _logger = logger;

    public async Task PublishAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        if (!configuration.TryGetTopicFromEventType<TEvent>(out string? topic))
        {
            _logger.LogCritical("No topic found for event type {EventType}", typeof(TEvent).Name);

            return;
        }

        await PublishAsync(key, @event, topic!, cancellationToken);
    }

    public async Task PublishDeleteAsync<TEvent>(Guid key, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        if (!configuration.TryGetTopicFromEventType<TEvent>(out string? topic))
        {
            _logger.LogCritical("No topic found for event type {EventType}", typeof(TEvent).Name);

            return;
        }

        await PublishDeleteAsync<TEvent>(key, topic!, cancellationToken);
    }

    public async Task PublishDeleteAsync<TEvent>(Guid key, string topic, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        using var producer = new ProducerBuilder<Guid, TEvent>(configuration.Producer)
            .SetLogHandler((_, logMessage) => _logger?.LogInformation(logMessage.Message))
            .SetErrorHandler((_, error) => _logger?.LogError(error.Reason))
            .SetKeySerializer(new GuidKeySerializer())
            .SetValueSerializer(new NullSerializer<TEvent>())
            .Build();

        await producer.ProduceAsync(topic,
            new Message<Guid, TEvent>()
            {
                Key = key,
                Timestamp = new Timestamp(DateTime.UtcNow)
            }, cancellationToken);

        producer.Flush(cancellationToken);
    }

    public async Task PublishAsync<TEvent>(Guid key, TEvent @event, string topic,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        using var producer = new ProducerBuilder<Guid, TEvent>(configuration.Producer)
            .SetLogHandler((_, logMessage) => _logger?.LogInformation(logMessage.Message))
            .SetErrorHandler((_, error) => _logger?.LogError(error.Reason))
            .SetKeySerializer(new GuidKeySerializer())
            .SetValueSerializer(new EventSerializer<TEvent>())
            .Build();

        await producer.ProduceAsync(topic,
            new Message<Guid, TEvent>()
            {
                Key = key,
                Timestamp = new Timestamp(@event.OccurredOn),
                Value = @event
            }, cancellationToken);

        producer.Flush(cancellationToken);
    }
}