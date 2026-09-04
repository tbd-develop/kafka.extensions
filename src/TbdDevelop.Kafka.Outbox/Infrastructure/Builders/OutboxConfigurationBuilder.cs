using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox.Infrastructure.Builders;

public class OutboxConfigurationBuilder<THostApplicationBuilder>(KafkaInstanceBuilder<THostApplicationBuilder> builder)
    where THostApplicationBuilder : IHostApplicationBuilder
{
    public OutboxConfigurationBuilder<THostApplicationBuilder> UseInMemoryOutbox()
    {
        builder.Register(services => { services.AddSingleton<IMessageOutbox, InMemoryMessageOutbox>(); });

        return this;
    }

    public OutboxConfigurationBuilder<THostApplicationBuilder> Register(
        Action<IKafkaServiceCollection> configure
    )
    {
        builder.Register(configure);

        return this;
    }
}