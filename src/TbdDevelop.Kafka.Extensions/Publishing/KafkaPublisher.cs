using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Serializers;

namespace TbdDevelop.Kafka.Extensions.Publishing;

public class KafkaPublisher : IEventPublisher
{
    private readonly ILogger _logger;
    private readonly KafkaConfiguration _configuration;

    public KafkaPublisher(ILogger<KafkaPublisher> logger, KafkaConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        if (!_configuration.TryGetTopicFromEventType<TEvent>(out string? topic))
        {
            _logger.LogCritical("No topic found for event type {EventType}", typeof(TEvent).Name);
        }

        using var producer = new ProducerBuilder<Guid, TEvent>(_configuration.Producer)
            .SetLogHandler((_, logMessage) => _logger?.LogInformation(logMessage.Message))
            .SetErrorHandler((_, error) => _logger?.LogError(error.Reason))
            .SetKeySerializer(new GuidKeySerializer())
            .SetValueSerializer(new EventSerializer<TEvent>())
            .Build();

        await producer.ProduceAsync(topic,
            new Message<Guid, TEvent>()
            {
                Key = @event.EventId,
                Timestamp = new Timestamp(@event.OccurredOn),
                Value = @event
            }, cancellationToken);

        producer.Flush(cancellationToken);
    }
}