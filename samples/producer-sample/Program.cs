using events;
using events.Envelopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Infrastructure;

var builder = Host.CreateApplicationBuilder();

builder
    .AddKafkaServices()
    .AddDefaultPublisher()
    .WithEnvelopeCodec<SampleEnvelopeCodec>();

var application = builder.Build();

var publisher = application.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(Guid.NewGuid(), new SampleEvent { SomeValue = "Hello World", SomeOtherValue = 42 });

await publisher.PublishAsync(Guid.NewGuid(), new SampleEvent { SomeValue = "Hello Another World", SomeOtherValue = 99 },
    "configured.topic");

await publisher.PublishAsync(Guid.NewGuid(), new SampleEnvelope<SampleEvent>
{
    Category = "test-events",
    Payload = new SampleEvent
    {
        SomeOtherValue = 94,
        SomeValue = "1042"
    }
}, "envelope.sample");

await publisher.PublishDeleteAsync<SampleEvent>(Guid.NewGuid());

await publisher.PublishDeleteAsync<SampleEvent>(Guid.NewGuid(), "configured.topic");


await application.RunAsync();