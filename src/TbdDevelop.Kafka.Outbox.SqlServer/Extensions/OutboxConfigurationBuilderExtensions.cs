using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.SqlServer.Context;
using TbdDevelop.Kafka.Outbox.SqlServer.Infrastructure;

namespace TbdDevelop.Kafka.Outbox.SqlServer.Extensions;

public static class OutboxConfigurationBuilderExtensions
{
    extension<THostApplicationBuilder>(
        OutboxConfigurationBuilder<THostApplicationBuilder> builder
    )
        where THostApplicationBuilder : IHostApplicationBuilder
    {
        public OutboxConfigurationBuilder<THostApplicationBuilder> UseSqlServerOutbox(
            string connectionString
        )
        {
            builder.Register(services =>
                ConfigureOutboxDbContext(services, new OutboxConfigurationOptions(connectionString)));

            return builder;
        }

        public OutboxConfigurationBuilder<THostApplicationBuilder> UseSqlServerOutbox(
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
            configure.UseSqlServer(options.ConnectionString);
        });

        services.AddInServiceLifetime<IMessageOutbox, SqlServerOutbox>();
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