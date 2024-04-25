using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Extensions.Configuration;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class KafkaConfigurationBuilder(IServiceCollection services)
{
    public KafkaConfigurationBuilder UsingAppSettings(string sectionName = "Kafka")
    {
        services.AddSingleton<KafkaConfiguration>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var config = new KafkaConfiguration();

            configuration.GetSection(sectionName).Bind(config);

            return config;
        });

        return this;
    }
    
    public KafkaConfigurationBuilder Register(Action<IServiceCollection> register)
    {
        register(services);

        return this;
    }
}