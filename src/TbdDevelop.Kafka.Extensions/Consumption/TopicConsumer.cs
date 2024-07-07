using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Deserializers;

namespace TbdDevelop.Kafka.Extensions.Consumption;

public class TopicConsumer<TEvent>(
    string topicToSubscribe,
    IDictionary<string, string> topicConfiguration,
    IEventReceiver<TEvent> eventReceiver,
    ILogger<TopicConsumer<TEvent>> logger)
    : ITopicConsumer
    where TEvent : class, IEvent
{
    private static JsonSerializerOptions DefaultJsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task Consume(CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Guid, string>(topicConfiguration)
            .SetKeyDeserializer(new GuidKeyDeserializer())
            .SetValueDeserializer(new StringDeserializer())
            .SetErrorHandler((_, error) => logger.LogError(error.Reason))
            .SetLogHandler((_, logMessage) => logger.LogInformation(logMessage.Message))
            .Build();

        consumer.Subscribe(topicToSubscribe);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = consumer.Consume(cancellationToken);

            if (result.Message is null) continue;

            var @event = JsonSerializer.Deserialize<TEvent>(result.Message.Value, DefaultJsonSerializerOptions);

            if (@event is null)
            {
                logger.LogError("{TopicToSubscribe} / {Message} message could not be deserialized",
                    topicToSubscribe,
                    result.Message.Value);

                continue;
            }

            await eventReceiver.ReceiveAsync(@event, cancellationToken);

            consumer.Commit(result);
        }
    }
}