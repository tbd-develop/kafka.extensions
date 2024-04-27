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
                    .UseInMemoryOutbox();
            }).AddOutboxPublishingService(configure =>
            {
                configure.WithSettings(settings => { settings.Interval = TimeSpan.FromSeconds(10); });
            });
    })
    .Build();

var publisher = host.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(Guid.NewGuid(),
    new SampleEvent { SomeValue = $"Hello, World {DateTime.UtcNow}", SomeOtherValue = 101 });

await host.RunAsync();