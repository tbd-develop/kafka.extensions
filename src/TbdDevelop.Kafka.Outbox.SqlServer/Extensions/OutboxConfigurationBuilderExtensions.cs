using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.SqlServer.Context;

namespace TbdDevelop.Kafka.Outbox.SqlServer.Extensions;

public static class OutboxConfigurationBuilderExtensions
{
    public static OutboxConfigurationBuilder UseSqlServerOutbox(this OutboxConfigurationBuilder builder,
        string connectionString)
    {
        builder.Register(services =>
        {
            services.AddPooledDbContextFactory<OutboxDbContext>(configure =>
                configure.UseSqlServer(connectionString));

            services.AddTransient<IMessageOutbox, SqlServerOutbox>();
        });

        return builder;
    }

    public static IHost ConfigureSqlOutbox(this IHost host)
    {
        var factory = host.Services.GetRequiredService<IDbContextFactory<OutboxDbContext>>();

        using var context = factory.CreateDbContext();

        context.Database.Migrate();

        return host;
    }
}