using System.Collections.ObjectModel;
using System.Reflection;
using JetBrains.Annotations;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Infrastructure.Exceptions;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class DispatchingConsumerBuilder(
    IKafkaServiceCollection collection)
{
    private readonly IDictionary<Type, IReadOnlyCollection<Type>> _consumerRegistrations = new Dictionary<Type, IReadOnlyCollection<Type>>();

    public DispatchingConsumerBuilder AddEventReceiver<TReceiver>()
        where TReceiver : class, IEventReceiver
    {
        var eventTypes =
            new ReadOnlyCollection<Type>([.. GetMessageTypes<TReceiver>()]);

        RegisterEventReceiver<TReceiver>();

        AddConsumerRegistration(typeof(TReceiver), eventTypes);

        return this;
    }
    public DispatchingConsumerOptions Build()
    {
        return new DispatchingConsumerOptions(_consumerRegistrations);
    }

    private void AddConsumerRegistration(
        Type receiverType,
        IReadOnlyCollection<Type> eventTypes
    )
    {
        if ( !_consumerRegistrations.TryAdd(receiverType, eventTypes) )
        {
            throw new ConsumerConfigurationException($"Receiver already registered - {receiverType}");
        }
    }

    private void RegisterEventReceiver<TReceiver>()
        where TReceiver : class, IEventReceiver
    {
        collection.AddInServiceLifetime<TReceiver>();
    }

    private IEnumerable<Type> GetMessageTypes<TReceiver>()
        where TReceiver : IEventReceiver
    {
        var eventTypes =
            GetEventTypes<TReceiver>(typeof(IEventReceiver<>))
                .Union(
                    GetEventTypes<TReceiver>(typeof(IReceive<>)));

        if ( eventTypes is null )
        {
            throw new TopicConfigurationException(
                $"Event Receiver {typeof(TReceiver).Name} does not implement IEventReceiver<TEvent>");
        }

        return eventTypes;
    }

    private static IEnumerable<Type> GetEventTypes<TReceiver>(
        Type eventInterface
    )
        where TReceiver : IEventReceiver
    {
        return from @interface in typeof(TReceiver).GetInterfaces()
            where @interface.IsGenericType &&
                  @interface.GetGenericTypeDefinition() == eventInterface
            let eventType = @interface?.GetGenericArguments()[0]
            where eventType is not null
            select eventType.IsGenericType ? eventType.GetGenericArguments()[0] : eventType;
    }
}