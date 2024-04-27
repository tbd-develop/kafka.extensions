using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Extensions.Configuration;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class OutboxPublishingConfigurationBuilder(IServiceCollection services)
{
    public OutboxPublishingConfigurationBuilder WithSettings(Action<OutboxPublishingConfiguration> configure)
    {
        services.Configure(configure);

        return this;
    }
}