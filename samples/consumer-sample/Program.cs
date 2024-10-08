﻿using consumer_sample.Handlers;
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