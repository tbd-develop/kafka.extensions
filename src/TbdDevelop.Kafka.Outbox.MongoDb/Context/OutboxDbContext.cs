using Microsoft.EntityFrameworkCore;
using TbdDevelop.Kafka.Outbox.MongoDb.Models;

namespace TbdDevelop.Kafka.Outbox.MongoDb.Context;

public class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessageContent> OutboxMessages => Set<OutboxMessageContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);
    }
}