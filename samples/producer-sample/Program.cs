using events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Infrastructure;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services => { services.AddKafka(builder => { builder.AddDefaultPublisher(); }); })
    .Build();

var publisher = host.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(new SampleEvent { SomeValue = "Hello World", SomeOtherValue = 42 });

//await host.RunAsync();