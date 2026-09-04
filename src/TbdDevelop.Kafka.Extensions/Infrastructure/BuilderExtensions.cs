using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public static class BuilderExtensions
{
    extension<THostApplicationBuilder>(
        THostApplicationBuilder builder
    )
        where THostApplicationBuilder : IHostApplicationBuilder
    {
        public KafkaInstanceBuilder<THostApplicationBuilder> AddKafkaServices(
            Action<KafkaInstanceBuilder<THostApplicationBuilder>>? configure = null
        )
        {
            var instanceBuilder = new KafkaInstanceBuilder<THostApplicationBuilder>(builder);

            configure?.Invoke(instanceBuilder);

            return instanceBuilder;
        }
    }
}