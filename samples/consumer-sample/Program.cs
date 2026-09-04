using consumer_sample.Receivers;
using events;
using events.Envelopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Services.Infrastructure;

var host = Host.CreateApplicationBuilder();

host.AddKafkaServices(configure => { configure.ServiceLifetime = ServiceLifetime.Scoped; })
    .AddDispatchingConsumer(configure => { configure.AddEventReceiver<SampleEventReceiver>(); })
    .AddBasicWorkerService();

host.Services.AddScoped<SampleEventReceiver>();
host.Services.AddSingleton<IPayloadTypeResolver>(new PayloadTypeResolver(new Dictionary<string, Type>
{
    [nameof(SampleEvent)] = typeof(SampleEvent)
}));

var application = host.Build();

await application.RunAsync();