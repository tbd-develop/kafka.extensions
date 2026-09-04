using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Extensions.Serializers;
using TbdDevelop.Kafka.Outbox.Configuration;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Outbox.Infrastructure;

public static class ServiceCollectionExtensions
{
    extension<THostApplicationBuilder>(
        KafkaInstanceBuilder<THostApplicationBuilder> builder
    )
        where THostApplicationBuilder : IHostApplicationBuilder
    {
        public KafkaInstanceBuilder<THostApplicationBuilder> AddOutboxPublisher(
            Action<OutboxConfigurationBuilder<THostApplicationBuilder>> configure
        )
        {
            builder.Register(services =>
            {
                services.GuardAlreadyRegistered<IEventPublisher>(
                    "Cannot add outbox publisher when a publisher is already registered");

                var outboxBuilder = new OutboxConfigurationBuilder<THostApplicationBuilder>(builder);

                configure(outboxBuilder);

                services.AddSingleton<IProducer<Guid, byte[]>>(provider =>
                {
                    var configuration = provider.GetRequiredService<IOptions<KafkaAppSettings>>();
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<KafkaPublisher>();

                    return new ProducerBuilder<Guid, byte[]>(configuration.Value.Producer)
                        .SetLogHandler((_, logMessage) => logger.LogInformation("{Message}", logMessage.Message))
                        .SetErrorHandler((_, error) => logger.LogError("{Reason}", error.Reason))
                        .SetKeySerializer(new GuidKeySerializer())
                        .Build();
                });

                services.AddInServiceLifetime<IEventPublisher, OutboxPublisher>();
                services.AddInServiceLifetime<KafkaPublisher>();
            });

            return builder;
        }

        public KafkaInstanceBuilder<THostApplicationBuilder> AddOutboxPublishingService(
            Action<OutboxPublishingConfigurationBuilder>? configure = null
        )
        {
            builder.Register(services =>
            {
                services.AddHostedService<OutboxService>();

                if ( configure is null )
                {
                    services.Configure<OutboxPublishingConfiguration>(_ => { });

                    return;
                }

                var builder = new OutboxPublishingConfigurationBuilder(services);

                configure(builder);
            });

            return builder;
        }
    }
}