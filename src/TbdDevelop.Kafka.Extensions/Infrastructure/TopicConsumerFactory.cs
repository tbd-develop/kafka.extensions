using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Consumption;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Extensions.Infrastructure.Exceptions;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public class TopicConsumerFactory(
    DispatchingConsumerOptions options,
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory,
    IOptions<KafkaAppSettings> configuration)
{
    public IEnumerable<ITopicConsumer> Create()
    {
        foreach ( var (receiverType, eventTypes) in options.Registrations )
        {
            var topic = FetchTopicFromEventTypes(eventTypes);

            if ( receiverType.IsSubclassOf(typeof(MultiEventReceiver)) )
            {
                var genericMethod = GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SingleOrDefault(m => m is { IsGenericMethod: true, Name: nameof(BuildMultiEventReceiver) })!
                    .MakeGenericMethod(receiverType);

                yield return (ITopicConsumer)genericMethod.Invoke(this, [topic])!;
            }
            else
            {
                var genericMethod = GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SingleOrDefault(m => m is { IsGenericMethod: true, Name: nameof(BuilderSingleEventReceiver) })!
                    .MakeGenericMethod(receiverType);

                yield return (ITopicConsumer)genericMethod.Invoke(this, [eventTypes.ElementAt(0), topic])!;
            }
        }
    }

    private string FetchTopicFromEventTypes(
        IReadOnlyCollection<Type> eventTypes
    )
    {
        var topics = eventTypes.Select(et =>
            {
                configuration.Value.TryGetTopicFromEventType(et, out var topic);

                return topic;
            })
            .Where(t => t is not null)
            .Distinct()
            .ToList();

        return topics.Count != 1
            ? throw new ConsumerConfigurationException($"No Topic available for given Event Types {string.Join(",", eventTypes.Select(s => s.Name))}")
            : topics[0]!;
    }

    private ITopicConsumer BuildMultiEventReceiver<TReceiver>(
        string topic
    )
        where TReceiver : IEventReceiver
    {
        return new MultiEventTopicConsumer(
            topic,
            configuration.Value.Consumer,
            serviceProvider.GetRequiredService<TReceiver>(),
            loggerFactory.CreateLogger<MultiEventTopicConsumer>(),
            serviceProvider.GetRequiredService<IEnvelopeCodec>(),
            serviceProvider.GetRequiredService<IPayloadTypeResolver>()
        );
    }

    private ITopicConsumer BuilderSingleEventReceiver<TReceiver>(
        Type eventType,
        string topic
    )
        where TReceiver : IEventReceiver
    {
#pragma warning disable S3011
        var method =
            Array.Find(
                    typeof(TopicConsumerFactory)
                        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance),
                    m => m.Name == nameof(BuildTopicConsumer) && m.GetGenericArguments().Length > 1)
                ?.MakeGenericMethod(eventType, typeof(TReceiver));

        if ( method is null )
        {
            throw new ReceiverConfigurationException("Unable to construct method for Event Receiver");
        }

        return (ITopicConsumer)method.Invoke(this, [topic])!;
    }

    private ITopicConsumer BuildTopicConsumer<TEvent, TReceiver>(
        string topic
    )
        where TEvent : class
        where TReceiver : class, IEventReceiver<TEvent>
    {
        return new TopicConsumer<TEvent, TReceiver>(
            topic,
            configuration.Value.Consumer,
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            loggerFactory.CreateLogger<TopicConsumer<TEvent, TReceiver>>(),
            serviceProvider.GetService<IEnvelopeCodec>()
        );
    }
}