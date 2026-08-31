using consumer_sample.Receivers;
using events;
using events.Envelopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Services.Infrastructure;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddScoped<SampleMultipleEventReceiver>();

        services.AddKafka()
            .AddDispatchingConsumer(configure => { configure.AddMultiEventReceiver<SampleMultipleEventReceiver>(); })
            .WithEnvelopeCodec<SampleEnvelopeCodec>()
            .AddBasicWorkerService();

        services.AddSingleton<IPayloadTypeResolver>(new PayloadTypeResolver(new Dictionary<string, Type>
        {
            [nameof(SampleEvent)] = typeof(SampleEvent)
        }));
    })
    .Build();

await host.RunAsync();