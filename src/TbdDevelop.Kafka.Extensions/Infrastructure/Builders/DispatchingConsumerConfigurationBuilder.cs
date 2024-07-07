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

    public DispatchingConsumerConfigurationBuilder AddEventReceiver<TEvent, TConsumer>(string? topic = null)
        where TEvent : class, IEvent
        where TConsumer : IEventReceiver<TEvent>
    {
        if (topic is null && !configuration.TryGetTopicFromEventType<TEvent>(out topic))
        {
            throw new TopicConfigurationException($"No topic found for event type {typeof(TEvent).Name}");
        }

        _consumers.Add(new TopicConsumer<TEvent>(
            topic!,
            configuration.Consumer,
            serviceProvider.GetRequiredService<TConsumer>(),
            loggerFactory.CreateLogger<TopicConsumer<TEvent>>()
        ));

        return this;
    }

    public IEventConsumer Build()
    {
        return new DispatchingKafkaConsumer(_consumers);
    }
}