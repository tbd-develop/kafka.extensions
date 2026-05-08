using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Outbox.Configuration;

namespace TbdDevelop.Kafka.Outbox.Infrastructure.Builders;

public class OutboxPublishingConfigurationBuilder(IServiceCollection services)
{
    public OutboxPublishingConfigurationBuilder WithSettings(Action<OutboxPublishingConfiguration> configure)
    {
        services.Configure(configure);

        return this;
    }
}