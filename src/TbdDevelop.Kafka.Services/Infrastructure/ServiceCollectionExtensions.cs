using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Extensions.Infrastructure;

namespace TbdDevelop.Kafka.Services.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static KafkaInstanceBuilder AddBasicWorkerService(this KafkaInstanceBuilder builder)
    {
        builder.Register(services => { services.AddHostedService<WorkerService>(); });

        return builder;
    }
}