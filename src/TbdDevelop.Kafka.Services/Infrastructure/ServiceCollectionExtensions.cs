using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Services.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static KafkaInstanceBuilder AddBasicWorkerService(this KafkaInstanceBuilder builder)
    {
        builder.Register(services => { services.AddHostedService<WorkerService>(); });

        return builder;
    }
}