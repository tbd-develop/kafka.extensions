using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Contracts;

namespace TbdDevelop.Kafka.Services;

public class WorkerService(IEventConsumer consumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.BeginConsumeAsync(stoppingToken);
    }
}