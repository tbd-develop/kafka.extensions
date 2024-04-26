using Microsoft.EntityFrameworkCore;
using TbdDevelop.Kafka.Outbox.SqlServer.Models;

namespace TbdDevelop.Kafka.Outbox.SqlServer.Context;

public class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessageContent> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);
    }
}