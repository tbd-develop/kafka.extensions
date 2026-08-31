using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Extensions.Serializers;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class KafkaInstanceBuilder(IServiceCollection services)
{
    public KafkaInstanceBuilder AddDefaultPublisher()
    {
        services.GuardAlreadyRegistered<IEventPublisher>(
            "Cannot register a default publisher when a publisher is already registered");

        services.AddSingleton<IProducer<Guid, byte[]>>(provider =>
        {
            var configuration = provider.GetRequiredService<KafkaConfiguration>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<KafkaPublisher>();

            return new ProducerBuilder<Guid, byte[]>(configuration.Producer)
                .SetLogHandler((_, logMessage) => logger.LogInformation("{Message}", logMessage.Message))
                .SetErrorHandler((_, error) => logger.LogError("{Reason}", error.Reason))
                .SetKeySerializer(new GuidKeySerializer())
                .Build();
        });

        services.AddScoped<IEventPublisher, KafkaPublisher>();

        return this;
    }

    public KafkaInstanceBuilder WithEnvelopeCodec<TCodec>()
        where TCodec : class, IEnvelopeCodec
    {
        services.AddSingleton<IEnvelopeCodec, TCodec>();

        return this;
    }

    public KafkaInstanceBuilder AddDispatchingConsumer(Action<DispatchingConsumerConfigurationBuilder> configure)
    {
        services.AddSingleton<IEventConsumer>(provider =>
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