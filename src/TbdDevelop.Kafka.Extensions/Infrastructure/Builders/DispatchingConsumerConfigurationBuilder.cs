using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Consumption;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Infrastructure.Exceptions;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class DispatchingConsumerConfigurationBuilder(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory,
    KafkaConfiguration configuration)
{
    private readonly List<ITopicConsumer> _consumers = [];

    public DispatchingConsumerConfigurationBuilder AddEventReceiver<TConsumer>(string? topic = null)
        where TConsumer : IEventReceiver
    {
        var eventType =
            Array.Find(typeof(TConsumer).GetInterfaces(),
                    m => m.IsGenericType && m.GetGenericTypeDefinition() == typeof(IEventReceiver<>))
                ?.GetGenericArguments()
                .FirstOrDefault();

        if (eventType is null)
        {
            throw new TopicConfigurationException(
                $"Event Receiver {typeof(TConsumer).Name} does not implement IEventReceiver<TEvent>");
        }

        InvokeAddEventReceiver<TConsumer>(eventType, topic);

        return this;
    }

    private void InvokeAddEventReceiver<TConsumer>(Type eventType, string? topic)
        where TConsumer : IEventReceiver
    {
#pragma warning disable S3011
        var method =
            Array.Find(
                    typeof(DispatchingConsumerConfigurationBuilder)
                        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance),
                    m => m.Name == nameof(AddEventReceiver) && m.GetGenericArguments().Length > 1)
                ?.MakeGenericMethod(eventType, typeof(TConsumer));

        method?.Invoke(this, [topic]);
    }

    private DispatchingConsumerConfigurationBuilder AddEventReceiver<TEvent, TConsumer>(string? topic = null)
        where TEvent : class
        where TConsumer : IEventReceiver<TEvent>
    {
        if (topic is null && !TryGetTopicNameFromEventType<TEvent>(out topic))
        {
            throw new TopicConfigurationException($"No topic found for event type {typeof(TEvent).Name}");
        }

        if (topic is not null)
        {
            configuration.TryGetTopicByName(topic, out topic);
        }

        _consumers.Add(new TopicConsumer<TEvent>(
            topic!,
            configuration.Consumer,
            serviceProvider.GetRequiredService<TConsumer>(),
            loggerFactory.CreateLogger<TopicConsumer<TEvent>>(),
            serviceProvider.GetService<IEnvelopeCodec>()
        ));

        return this;
    }

    private bool TryGetTopicNameFromEventType<TEvent>(out string? topic)
    {
        var type = typeof(TEvent);

        type = type.IsGenericType ? type.GetGenericArguments()[0] : type;

        return configuration.TryGetTopicFromEventType(type, out topic);
    }

    public IEventConsumer Build()
    {
        return new DispatchingKafkaConsumer(
            loggerFactory.CreateLogger<DispatchingKafkaConsumer>(),
            _consumers);
    }
}