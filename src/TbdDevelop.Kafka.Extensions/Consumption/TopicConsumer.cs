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
    public string Topic => topicToSubscribe;

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
                logger.LogCritical(ex, "Failed to deserialize message on {Topic}, skipping.", topicToSubscribe);

                consumer.Commit(result);
            }
        }
    }

    private async Task<bool> HandleMessage(ConsumeResult<Guid, string> result, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(result.Message.Value))
        {
            var @event = JsonSerializer.Deserialize<TEvent>(result.Message.Value, DefaultJsonSerializerOptions);

            if (@event is null)
            {
                logger.LogError("{TopicToSubscribe} / {Message} message could not be deserialized",
                    topicToSubscribe,
                    result.Message.Value);

                return false;
            }

            await eventReceiver.ReceiveAsync(@event, cancellationToken);
        }
        else
        {
            await eventReceiver.DeleteAsync(result.Message.Key, cancellationToken);
        }

        return true;
    }
}