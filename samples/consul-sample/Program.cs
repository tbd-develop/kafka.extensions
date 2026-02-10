using events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Configuration.Consul.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddKafka(configure =>
            {
                configure.UsingConsul(new ConsulConfiguration(
                    "http://localhost:8599",
                    "Kafka",
                    "kafka-configuration"));
            })
            .AddDefaultPublisher();
    })
    .Build();

var publisher = host.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(Guid.NewGuid(), new SampleEvent { SomeValue = "Hello World", SomeOtherValue = 42 });