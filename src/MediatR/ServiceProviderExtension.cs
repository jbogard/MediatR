using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MediatR;

internal static class ServiceProviderExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetService<T>(this IServiceProvider serviceProvider) =>
        (T?)serviceProvider.GetService(typeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetRequiredService<T>(this IServiceProvider serviceProvider) =>
        serviceProvider.GetService<T>() ?? ThrowServiceNotFound<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetServices<T>(this IServiceProvider serviceProvider) => 
        serviceProvider.GetRequiredService<IEnumerable<T>>() as T[] ?? Array.Empty<T>();

    private static T ThrowServiceNotFound<T>() =>
        throw new InvalidOperationException($"Could not find service '{typeof(T)}' registered in the container.");
}