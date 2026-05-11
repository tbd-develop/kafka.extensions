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

    public DispatchingConsumerConfigurationBuilder AddMultiEventReceiver<TReceiver>()
        where TReceiver : IEventReceiver
    {
        var eventTypes = from @interface in typeof(TReceiver).GetInterfaces()
            where @interface.IsGenericType &&
                  @interface.GetGenericTypeDefinition() == typeof(IReceive<>)
            let eventType = @interface?.GetGenericArguments()[0]
            where eventType is not null
            select eventType.IsGenericType ? eventType.GetGenericArguments()[0] : eventType;

        var topics = eventTypes.Select(et =>
            {
                configuration.TryGetTopicFromEventType(et, out var topic);

                return topic;
            })
            .Where(t => t is not null)
            .Distinct()
            .ToList();

        if (topics.Count != 1)
        {
            throw new ConsumerConfigurationException($"Events for {typeof(TReceiver)} must come from same topic");
        }

        _consumers.Add(new MultiEventTopicConsumer(
            topics[0]!,
            configuration.Consumer,
            serviceProvider.GetRequiredService<TReceiver>(),
            loggerFactory.CreateLogger<MultiEventTopicConsumer>(),
            serviceProvider.GetRequiredService<IEnvelopeCodec>(),
            serviceProvider.GetRequiredService<IPayloadTypeResolver>()
        ));

        return this;
    }

    public DispatchingConsumerConfigurationBuilder AddEventReceiver<TReceiver>()
        where TReceiver : IEventReceiver
    {
        var eventType =
            Array.Find(typeof(TReceiver).GetInterfaces(),
                    m => m.IsGenericType && m.GetGenericTypeDefinition() == typeof(IEventReceiver<>))
                ?.GetGenericArguments()
                .FirstOrDefault();

        if (eventType is null)
        {
            throw new TopicConfigurationException(
                $"Event Receiver {typeof(TReceiver).Name} does not implement IEventReceiver<TEvent>");
        }

        InvokeAddEventReceiver<TReceiver>(eventType);

        return this;
    }

    private void InvokeAddEventReceiver<TReceiver>(Type eventType)
        where TReceiver : IEventReceiver
    {
#pragma warning disable S3011
        var method =
            Array.Find(
                    typeof(DispatchingConsumerConfigurationBuilder)
                        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance),
                    m => m.Name == nameof(AddEventReceiver) && m.GetGenericArguments().Length > 1)
                ?.MakeGenericMethod(eventType, typeof(TReceiver));

        method?.Invoke(this, []);
    }

    private DispatchingConsumerConfigurationBuilder AddEventReceiver<TEvent, TReceiver>()
        where TEvent : class
        where TReceiver : IEventReceiver<TEvent>
    {
        if (!TryGetTopicNameFromEventType<TEvent>(out string? topic))
        {
            throw new TopicConfigurationException($"No topic found for event type {typeof(TEvent).Name}");
        }

        _consumers.Add(new TopicConsumer<TEvent>(
            topic!,
            configuration.Consumer,
            serviceProvider.GetRequiredService<TReceiver>(),
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