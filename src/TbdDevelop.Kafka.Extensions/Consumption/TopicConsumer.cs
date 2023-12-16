using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Deserializers;

namespace TbdDevelop.Kafka.Extensions.Consumption;

public abstract class TopicConsumer
{
    public abstract Task Consume(CancellationToken cancellationToken);
}

public class TopicConsumer<TEvent> : TopicConsumer
    where TEvent : class, IEvent
{
    private readonly string _topicToSubscribe;
    private readonly IDictionary<string, string> _topicConfiguration;
    private readonly IEventReceiver<TEvent> _eventReceiver;
    private readonly ILogger<TopicConsumer<TEvent>> _logger;

    public TopicConsumer(
        string topicToSubscribe,
        IDictionary<string, string> topicConfiguration,
        IEventReceiver<TEvent> eventReceiver,
        ILogger<TopicConsumer<TEvent>> logger)
    {
        _topicToSubscribe = topicToSubscribe;
        _topicConfiguration = topicConfiguration;
        _eventReceiver = eventReceiver;
        _logger = logger;
    }

    public override async Task Consume(CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Guid, string>(_topicConfiguration)
            .SetKeyDeserializer(new GuidKeyDeserializer())
            .SetValueDeserializer(new StringDeserializer())
            .SetErrorHandler((_, error) => _logger.LogError(error.Reason))
            .SetLogHandler((_, logMessage) => _logger.LogInformation(logMessage.Message))
            .Build();

        consumer.Subscribe(_topicToSubscribe);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = consumer.Consume(cancellationToken);

            if (result.Message is null) continue;

            var @event = JsonSerializer.Deserialize<TEvent>(result.Message.Value,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            if (@event is null)
            {
                _logger.LogError("{topicToSubscribe} / {message} message could not be deserialized",
                    _topicToSubscribe,
                    result.Message.Value);

                continue;
            }

            await _eventReceiver.ReceiveAsync(@event, cancellationToken);

            consumer.Commit(result);
        }
    }
}