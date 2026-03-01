using Microsoft.EntityFrameworkCore;
using TbdDevelop.Kafka.Outbox.Postgres.Models;

namespace TbdDevelop.Kafka.Outbox.Postgres.Context;

public class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessageContent> OutboxMessages => Set<OutboxMessageContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("outbox")
            .ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);
    }
}