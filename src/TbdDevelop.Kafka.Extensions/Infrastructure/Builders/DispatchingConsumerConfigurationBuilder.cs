﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Consumption;
using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class DispatchingConsumerConfigurationBuilder(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory,
    KafkaConfiguration configuration)
{
    private readonly List<TopicConsumer> _consumers = [];

    public DispatchingConsumerConfigurationBuilder AddEventReceiver<TEvent, TConsumer>()
        where TEvent : class, IEvent
        where TConsumer : IEventReceiver<TEvent>
    {
        if (configuration.TryGetTopicFromEventType<TEvent>(out string? topic))
        {
            _consumers.Add(new TopicConsumer<TEvent>(
                topic!,
                configuration.Consumer,
                serviceProvider.GetRequiredService<TConsumer>(),
                loggerFactory.CreateLogger<TopicConsumer<TEvent>>()
            ));
        }

        return this;
    }

    public IEventConsumer Build()
    {
        return new DispatchingKafkaConsumer(_consumers);
    }
}