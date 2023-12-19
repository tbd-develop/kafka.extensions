using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

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

    public static void GuardAlreadyRegistered<TService>(this IServiceCollection services, string message)
    {
        if (services.Any(s => s.ServiceType == typeof(TService)))
        {
            throw new InvalidOperationException(message);
        }
    }
}