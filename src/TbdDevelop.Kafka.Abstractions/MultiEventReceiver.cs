using System.Reflection;

namespace TbdDevelop.Kafka.Abstractions;

public abstract class MultiEventReceiver : IEventReceiver
{
    private IDictionary<Type, Type>? _interfaces;
    private IDictionary<Type, MethodInfo>? _methods;

    public async Task ReceiveAsync(
        object @event,
        CancellationToken cancellationToken = default
    )
    {
        PopulateAvailableMethods();

        if ( _methods is not null && _methods.TryGetValue(@event.GetType(), out var methodToInvoke) )
        {
            await (Task)methodToInvoke.Invoke(this, [@event, cancellationToken])!;
        }
    }

    private void PopulateAvailableMethods()
    {
        _interfaces ??=
            (from @interface in GetType().GetInterfaces()
                where @interface.IsGenericType &&
                      @interface.GetGenericTypeDefinition() == typeof(IReceive<>)
                select @interface)
            .ToDictionary(k => k.GetGenericArguments()[0], k => k);

        _methods ??= (from method in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where method.Name == "ReceiveAsync"
                let parameterType = method.GetParameters().FirstOrDefault()?.ParameterType
                where parameterType is not null && _interfaces.ContainsKey(parameterType)
                select new { Key = parameterType, Method = method })
            .ToDictionary(k => k.Key, k => k.Method);
    }

    public Task DeleteAsync(
        Guid key,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }
}