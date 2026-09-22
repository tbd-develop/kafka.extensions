using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Consumption;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Extensions.Serializers;

namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public class KafkaInstanceBuilder<THostApplicationBuilder>(THostApplicationBuilder builder)
    where THostApplicationBuilder : IHostApplicationBuilder
{
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Transient;
    private IKafkaServiceCollection ServiceCollection { get; set; } = null!;
    private IConfiguration Configuration { get; set; } = null!;
    private string _appSettingsSectionName = null!;

    public KafkaInstanceBuilder<THostApplicationBuilder> Build()
    {
        Configuration = builder.Configuration;

        ServiceCollection = new KafkaServiceCollection(ServiceLifetime, builder.Services);

        ServiceCollection.Configure<KafkaAppSettings>(
            Configuration.GetSection(_appSettingsSectionName)
        );

        return this;
    }

    public KafkaInstanceBuilder<THostApplicationBuilder> UseAppSettings(
        string sectionName
    )
    {
        _appSettingsSectionName = sectionName;

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
        Action<DispatchingConsumerBuilder> configure
    )
    {
        var consumerBuilder = new DispatchingConsumerBuilder(ServiceCollection);

        configure(consumerBuilder);

        ServiceCollection.AddSingleton(consumerBuilder.Build());
        ServiceCollection.AddSingleton<TopicConsumerFactory>();
        ServiceCollection.AddSingleton<IEventConsumer, DispatchingKafkaConsumer>();

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
            var logger = loggerFactory.CreateLogger<KafkaInstanceBuilder<THostApplicationBuilder>>();

            return new ProducerBuilder<Guid, byte[]>(configuration.Value.Producer)
                .SetLogHandler((_, logMessage) => logger.LogInformation("{Message}", logMessage.Message))
                .SetErrorHandler((_, error) => logger.LogError("{Reason}", error.Reason))
                .SetKeySerializer(new GuidKeySerializer())
                .Build();
        });

        services.AddSingleton<IEventPublisher, KafkaPublisher>();
    }

    public KafkaInstanceBuilder<THostApplicationBuilder> Register(
        Action<IKafkaServiceCollection> configure
    )
    {
        configure(ServiceCollection);

        return this;
    }
}