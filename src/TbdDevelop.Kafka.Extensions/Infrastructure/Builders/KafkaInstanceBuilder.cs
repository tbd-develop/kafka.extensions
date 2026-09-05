using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Extensions.Serializers;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class KafkaInstanceBuilder<THostApplicationBuilder>(THostApplicationBuilder builder)
    where THostApplicationBuilder : IHostApplicationBuilder
{
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Transient;
    private IKafkaServiceCollection ServiceCollection { get; set; }
    private IConfiguration Configuration { get; set; }

    public KafkaInstanceBuilder<THostApplicationBuilder> UseAppSettings(
        string sectionName
    )
    {
        ServiceCollection.Configure<KafkaAppSettings>(
            Configuration.GetSection(sectionName)
        );

        return this;
    }

    public KafkaInstanceBuilder<THostApplicationBuilder> AddDefaultPublisher()
    {
        RegisterDefaultPublisher(ServiceCollection);

        return this;
    }

    public KafkaInstanceBuilder<THostApplicationBuilder> WithEnvelopeCodec<TCodec>()
        where TCodec : class, IEnvelopeCodec
    {
        ServiceCollection.AddSingleton<IEnvelopeCodec, TCodec>();

        return this;
    }

    public KafkaInstanceBuilder<THostApplicationBuilder> AddDispatchingConsumer(
        Action<DispatchingConsumerConfigurationBuilder> configure
    )
    {
        RegisterDispatchingConsumer(ServiceCollection, configure);

        return this;
    }

    private void RegisterDefaultPublisher(
        IKafkaServiceCollection services
    )
    {
        services.GuardAlreadyRegistered<IEventPublisher>(
            "Cannot register a default publisher when a publisher is already registered");

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

        services.AddInServiceLifetime<IEventPublisher, KafkaPublisher>();
    }

    private void RegisterDispatchingConsumer(
        IServiceCollection services,
        Action<DispatchingConsumerConfigurationBuilder> configure
    )
    {
        services.AddSingleton<DispatchingConsumerConfigurationBuilder>();
        services.AddSingleton<IEventConsumer>(provider =>
        {
            var consumerConfigurationBuilder =
                provider.GetRequiredService<DispatchingConsumerConfigurationBuilder>();

            configure(consumerConfigurationBuilder);

            return consumerConfigurationBuilder.Build();
        });
    }

    public KafkaInstanceBuilder<THostApplicationBuilder> Register(
        Action<IKafkaServiceCollection> configure
    )
    {
        configure(ServiceCollection);

        return this;
    }

    public KafkaInstanceBuilder<THostApplicationBuilder> Build()
    {
        Configuration = builder.Configuration;

        ServiceCollection = new KafkaServiceCollection(ServiceLifetime, builder.Services);

        return this;
    }
}