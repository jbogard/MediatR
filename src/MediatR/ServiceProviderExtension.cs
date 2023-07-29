using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MediatR;

internal static class ServiceProviderExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetService<T>(this IServiceProvider serviceProvider) =>
        (T?)serviceProvider.GetService(typeof(T));

    public static T GetRequiredService<T>(this IServiceProvider serviceProvider) =>
        (T?)serviceProvider.GetService(typeof(T)) ?? ThrowServiceNotFound<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetServices<T>(this IServiceProvider serviceProvider)
    {
        var services = (IEnumerable<T>)serviceProvider.GetService(typeof(IEnumerable<T>));
        if (services is T[] arrayServices)
        {
            return arrayServices;
        }

        return services?.ToArray() ?? Array.Empty<T>();
    }

    private static T ThrowServiceNotFound<T>() =>
        throw new InvalidOperationException($"Could not find service '{typeof(T)}' registered in the container.");
}