using events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Infrastructure;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services
            .AddKafka()
            .AddDefaultPublisher();
    })
    .Build();

var publisher = host.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(Guid.NewGuid(), new SampleEvent { SomeValue = "Hello World", SomeOtherValue = 42 });

await publisher.PublishAsync(Guid.NewGuid(), new SampleEvent { SomeValue = "Hello Another World", SomeOtherValue = 99 },
    "configured.topic");

await host.RunAsync();