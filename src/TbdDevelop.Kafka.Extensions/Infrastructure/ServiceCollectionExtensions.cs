using Microsoft.Extensions.DependencyInjection;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void GuardAlreadyRegistered<TService>(
        this IServiceCollection services,
        string message
    )
    {
        if ( services.Any(s => s.ServiceType == typeof(TService)) )
        {
            throw new InvalidOperationException(message);
        }
    }
}