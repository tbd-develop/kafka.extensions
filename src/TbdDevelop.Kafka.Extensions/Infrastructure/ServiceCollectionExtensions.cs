using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Consumption.Builders;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Publishing;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services, Action<KafkaInstanceBuilder> builder)
    {
        var kafkaBuilder = new KafkaInstanceBuilder(services);

        services.AddSingleton<KafkaConfiguration>((provider) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var config = new KafkaConfiguration();

            configuration.GetSection("Kafka").Bind(config);

            return config;
        });

        builder(kafkaBuilder);

        return services;
    }
}

public class KafkaInstanceBuilder(IServiceCollection services)
{
    public KafkaInstanceBuilder AddDefaultPublisher()
    {
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