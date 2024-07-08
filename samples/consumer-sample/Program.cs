// See https://aka.ms/new-console-template for more information

using consumer_sample.Handlers;
using events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Services.Infrastructure;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<SampleEventReceiver>();

        services.AddKafka()
            .AddDispatchingConsumer(configure =>
            {
                configure.AddEventReceiver<SampleEventReceiver>();
                configure.AddEventReceiver<SampleEventReceiver>("configured.topic");
            })
            .AddBasicWorkerService();
    })
    .Build();

await host.RunAsync();