﻿using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Configuration;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Outbox.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static KafkaInstanceBuilder AddOutboxPublisher(this KafkaInstanceBuilder builder,
        Action<OutboxConfigurationBuilder> configure)
    {
        builder.Register(services =>
        {
            services.GuardAlreadyRegistered<IEventPublisher>(
                "Cannot add outbox publisher when a publisher is already registered");

            var outboxBuilder = new OutboxConfigurationBuilder(services);

            configure(outboxBuilder);

            services.AddTransient<IEventPublisher, OutboxPublisher>();
            services.AddTransient<KafkaPublisher>();
        });

        return builder;
    }

    public static KafkaInstanceBuilder AddOutboxPublishingService(
        this KafkaInstanceBuilder builder,
        Action<OutboxPublishingConfigurationBuilder>? configure = null)
    {
        builder.Register(services =>
        {
            services.AddHostedService<OutboxService>();

            if (configure is null)
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