using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Configuration.Consul;

public static partial class KafkaConfigurationBuilderExtensions
{
    extension<THostApplicationBuilder>(
        KafkaInstanceBuilder<THostApplicationBuilder> builder
    )
        where THostApplicationBuilder : IHostApplicationBuilder
    {
        public KafkaInstanceBuilder<THostApplicationBuilder> UsingConsul(
            ConsulConfiguration configuration
        )
        {
            builder.Register(services =>
            {
                services.AddHttpClient<ConsulClient>(client =>
                {
                    client.BaseAddress = new Uri(configuration.Address);
                });

                services.AddSingleton<IOptions<KafkaAppSettings>>(provider =>
                {
                    var client = provider.GetRequiredService<ConsulClient>();
                    var appConfiguration = provider.GetRequiredService<IConfiguration>();

                    var config = new KafkaAppSettings();

                    appConfiguration
                        .GetSection(configuration.KafkaAppSettingsSectionName)
                        .Bind(config);

                    config.Topics = client.GetConfiguration(configuration.Key)
                        .GetAwaiter()
                        .GetResult()
                        .Topics;

                    return new OptionsWrapper<KafkaAppSettings>(config);
                });
            });

            return builder;
        }
    }
}