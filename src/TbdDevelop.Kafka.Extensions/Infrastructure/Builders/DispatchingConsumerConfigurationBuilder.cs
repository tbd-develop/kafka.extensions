using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Consumption;
using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class DispatchingConsumerConfigurationBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    private readonly List<TopicConsumer> _consumers;

    public DispatchingConsumerConfigurationBuilder(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        KafkaConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;

        _loggerFactory = loggerFactory;
        _consumers = new List<TopicConsumer>();
    }

    public DispatchingConsumerConfigurationBuilder AddEventReceiver<TEvent, TConsumer>()
        where TEvent : class, IEvent
        where TConsumer : IEventReceiver<TEvent>
    {
        if (_configuration.TryGetTopicFromEventType<TEvent>(out string? topic))
        {
            _consumers.Add(new TopicConsumer<TEvent>(
                topic!,
                _configuration.Consumer,
                _serviceProvider.GetRequiredService<TConsumer>(),
                _loggerFactory.CreateLogger<TopicConsumer<TEvent>>()
            ));
        }

        return this;
    }

    public IEventConsumer Build()
    {
        return new DispatchingKafkaConsumer(_consumers);
    }
}