using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.Postgres.Context;
using TbdDevelop.Kafka.Outbox.Postgres.Infrastructure;

namespace TbdDevelop.Kafka.Outbox.Postgres.Extensions;

public static class OutboxConfigurationBuilderExtensions
{
    extension<THostApplicationBuilder>(
        OutboxConfigurationBuilder<THostApplicationBuilder> builder
    )
        where THostApplicationBuilder : IHostApplicationBuilder
    {
        public OutboxConfigurationBuilder<THostApplicationBuilder> UseNpgSqlOutbox(
            string connectionString
        )
        {
            builder.Register(services =>
                ConfigureOutboxDbContext(services, new OutboxConfigurationOptions(connectionString)));

            return builder;
        }

        public OutboxConfigurationBuilder<THostApplicationBuilder> UseNpgSqlOutbox(
            OutboxConfigurationOptions options
        )
        {
            builder.Register(services =>
                ConfigureOutboxDbContext(services, options));

            return builder;
        }
    }

    private static void ConfigureOutboxDbContext(
        IKafkaServiceCollection services,
        OutboxConfigurationOptions options
    )
    {
        services.AddPooledDbContextFactory<OutboxDbContext>(configure =>
        {
            configure.UseNpgsql(options.ConnectionString);
        });

        services.AddInServiceLifetime<IMessageOutbox, PostgresOutbox>();
    }

    public static IHost ConfigureKafkaSqlOutbox(
        this IHost host
    )
    {
        var factory = host.Services.GetRequiredService<IDbContextFactory<OutboxDbContext>>();

        using var context = factory.CreateDbContext();

        context
            .Database
            .Migrate();

        return host;
    }
}