using events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Configuration.Consul;
using TbdDevelop.Kafka.Extensions.Infrastructure;

var builder = Host.CreateApplicationBuilder();

builder.AddKafkaServices(configure =>
    {
        configure.ServiceLifetime = ServiceLifetime.Scoped;

        configure.UseAppSettings("Kafka");

        configure.UsingConsul(new ConsulConfiguration(
            "http://devstation:8500",
            "Kafka",
            "kafka-configuration"));
    })
    .AddDefaultPublisher();

var host = builder.Build();

var publisher = host.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(Guid.NewGuid(), new SampleEvent { SomeValue = "Hello From Consul Configuration", SomeOtherValue = 42 });