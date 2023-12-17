## Kakfa Extensions

This library contains a set of tools I use often to work with Apache Kafka. More specifically, 
it's build off of the Confluent Kafka library. 

### Why?

I always want to clean up using Kafka, and I want to avoid using the consumers / producer components 
in all my code. So, I often end up writing pieces to wrap around them that look like this. This is just 
a result of several attempts to make something smoother.

### How do I use it?

#### Configuration

First you're going to need the configuration set up. Add a node to your appsettings configuration as such;

```
"Kafka": {
    "Producer": {
      "bootstrap.servers": "localhost:9092"
    },
   "Consumer": {
      "group.id": "group-id",
      "bootstrap.servers": "localhost:9092",
      "auto.offset.reset": "earliest"
    },
    "Topics": [
      {
        "TypeName": "events.ExampleEvent, events",
        "Name": "my-product.first-event"
      },
      {
        "TypeName": "events.AnotherEvent, events",
        "Name": "my-product.second-event"
      }
    ] 
}
```

Obviously, if you're publishing, then you need Producer. Consuming, you need consumer. In either case you need the topics 
configured that you're going to use. 

Here, the events that we're publishing / consuming live in a shared events library that you're application knows about.

The configuration under consumer and producer are standard Kafka configuration value options. If you can pass them to a Kafka 
producer on consumer, you can put them in here.

#### Publishing (Producers)

If you want to configure your application to Publish events, then you need to add the production components. So, in your 
services configuration for Dependency Injection, you need

```csharp
services.AddKafka(builder => {
    builder.AddDefaultPublisher();    
    });
```

This will set up the configuration, and load a IEventPublisher. To use the publisher, use as such;

```csharp
public class MyService 
{
    private readonly IEventPublisher _publisher;
    
    public MyService(IEventPublisher publisher)
    {
        _publisher = publisher;
    }
    
    public async Task DoSomething()
    {
        await _publisher.Publish(new ExampleEvent());
    }
}
```    

Where ExampleEvent is a class that implements IEvent. DefaultEvent is an abstract implementation 
of IEvent to use as a base class.  

#### Consuming (Consumers)

If you want to configure your application to consume events, then you need to add the consumer components. For each event you wish to receive, 
you need to register a receiver. In your services configuration for Dependency Injection, you need

```csharp
services.AddKafka(builder => {
    builder.AddDispatchingConsumer( configure => {
            configure.AddEventReceiver<ExampleEvent, ExampleEventReceiver>();
            configure.AddEventReceiver<AnotherEvent, AnotherEventReceiver>();
    });    
});
```
Your receivers should be added to DI before kafka configuration, it's ok to register them as singletons 
as each receiver will be attached to a single running consumer process. 

```csharp
services.AddSingleton<ExampleEventReceiver>();
services.AddSingleton<AnotherEventReceiver>();
```

But, once configured, when the consumers are started, and an event is received then they'll be dispatched to the handlers.

There is a default worker service available in the services library. With it included, you can add this using

```csharp
services.AddKafka(builder => {
    builder.AddDispatchingConsumer( configure => {
            configure.AddEventReceiver<ExampleEvent, ExampleEventHandler>();
            configure.AddEventReceiver<AnotherEvent, AnotherEventHandler>();
    });    
    builder.AddBasicWorkerService();
});
```

This will add a hosted service and then start consuming when the service starts. 