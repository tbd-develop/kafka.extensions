using Microsoft.EntityFrameworkCore;
using TbdDevelop.Kafka.Outbox.SqlServer.Models;

namespace TbdDevelop.Kafka.Outbox.SqlServer.Context;

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