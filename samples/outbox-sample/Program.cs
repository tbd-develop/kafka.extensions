using events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Outbox.Infrastructure;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddKafka()
            .AddOutboxPublisher(configure =>
            {
                configure
                    .UseInMemoryOutbox()
                    .WithDefaultPublisher();
            });
    })
    .Build();

var publisher = host.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(new SampleEvent { SomeValue = "Hello, World", SomeOtherValue = 101 });

await host.RunAsync();