﻿using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;

namespace TbdDevelop.Kafka.Outbox.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static KafkaInstanceBuilder AddOutbox(this KafkaInstanceBuilder builder,
        Action<OutboxConfigurationBuilder> configure)
    {
        builder.Register(services =>
        {
            services.GuardAlreadyRegistered<IEventPublisher>(
                "Cannot add outbox when a publisher is already registered");

            var outboxBuilder = new OutboxConfigurationBuilder(services);

            configure(outboxBuilder);

            services.AddHostedService<OutboxService>();
        });

        return builder;
    }
}