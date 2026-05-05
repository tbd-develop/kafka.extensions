using consumer_sample.Receivers;
using events.Envelopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Services.Infrastructure;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddTransient<SampleEnvelopeReceiver>();

        services.AddKafka()
            .AddDispatchingConsumer(configure =>
            {
                configure.AddEventReceiver<SampleEnvelopeReceiver>("enveloped-topics");
            })
            .WithEnvelopeCodec<SampleEnvelopeCodec>()
            .AddBasicWorkerService();
    })
    .Build();

await host.RunAsync();