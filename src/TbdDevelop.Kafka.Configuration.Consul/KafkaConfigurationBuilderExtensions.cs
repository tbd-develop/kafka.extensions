using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Configuration.Consul.Infrastructure;

public static partial class KafkaConfigurationBuilderExtensions
{
    public static KafkaConfigurationBuilder UsingConsul(this KafkaConfigurationBuilder builder,
        ConsulConfiguration configuration)
    {
        builder.Register(services =>
        {
            services.AddHttpClient<ConsulClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.Address);
            });

            services.AddSingleton<KafkaConfiguration>(provider =>
            {
                var client = provider.GetRequiredService<ConsulClient>();
                var appConfiguration = provider.GetRequiredService<IConfiguration>();

                var config = new KafkaConfiguration();

                appConfiguration
                    .GetSection(configuration.KafkaAppSettingsSectionName)
                    .Bind(config);

                config.Topics = client.GetConfiguration(configuration.Key)
                    .GetAwaiter()
                    .GetResult()
                    .Topics;

                return config;
            });
        });

        return builder;
    }
}