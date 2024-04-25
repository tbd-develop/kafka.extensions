using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static KafkaInstanceBuilder AddKafka(this IServiceCollection services,
        Action<KafkaConfigurationBuilder>? builder = null)
    {
        var kafkaBuilder = new KafkaConfigurationBuilder(services);

        if (builder is not null)
        {
            builder(kafkaBuilder);
        }
        else
        {
            kafkaBuilder.UsingAppSettings();
        }

        return new KafkaInstanceBuilder(services);
    }

    public static void GuardAlreadyRegistered<TService>(this IServiceCollection services, string message)
    {
        if (services.Any(s => s.ServiceType == typeof(TService)))
        {
            throw new InvalidOperationException(message);
        }
    }
}