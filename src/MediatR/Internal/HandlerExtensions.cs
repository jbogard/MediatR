using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MediatR.Internal;

/// <summary>
/// Contains utility methods for working with implementations of request
/// handlers.
/// </summary>
public static class HandlerExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> PropertyCache = new();

    /// <summary>
    /// Inspects the <paramref name="value"/> parameter for a public Order property
    /// that is gettable and an integer.  If found, it uses the value of this property,
    /// otherwise returning null.
    /// </summary>
    /// <param name="value">The value to inspect.</param>
    /// <returns>The value of the "Order" property or null if not found.</returns>
    public static int? GetOrderIfExists(this object value)
    {
        var orderProperty = PropertyCache.GetOrAdd(value.GetType(), valueType =>
        {
            var orderProperty = valueType.GetProperty("Order");
            return orderProperty ?? null;
        });

        var orderValue = orderProperty?.GetValue(value);
        if (orderValue is not int i) return null;

        return i;
    }
}
