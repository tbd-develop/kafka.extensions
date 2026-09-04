using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Services.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static KafkaInstanceBuilder<THostApplicationBuilder> AddBasicWorkerService<THostApplicationBuilder>(
        this KafkaInstanceBuilder<THostApplicationBuilder> builder
    )
        where THostApplicationBuilder : IHostApplicationBuilder
    {
        builder.Register(services => { services.AddHostedService<WorkerService>(); });

        return builder;
    }
}