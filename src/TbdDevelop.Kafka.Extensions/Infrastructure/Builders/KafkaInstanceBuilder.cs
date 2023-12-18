using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Publishing;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class KafkaInstanceBuilder(IServiceCollection services)
{
    public KafkaInstanceBuilder AddDefaultPublisher()
    {
        services.GuardAlreadyRegistered<IEventPublisher>(
            "Cannot register a default publisher when a publisher is already registered");

        services.AddTransient<IEventPublisher, KafkaPublisher>();

        return this;
    }

    public KafkaInstanceBuilder AddDispatchingConsumer(Action<DispatchingConsumerConfigurationBuilder> configure)
    {
        services.AddSingleton<IEventConsumer>((provider) =>
        {
            var builder =
                new DispatchingConsumerConfigurationBuilder(
                    provider,
                    provider.GetRequiredService<ILoggerFactory>(),
                    provider.GetRequiredService<KafkaConfiguration>());

            configure(builder);

            return builder.Build();
        });

        return this;
    }

    public KafkaInstanceBuilder Register(Action<IServiceCollection> register)
    {
        register(services);

        return this;
    }
}