using System;
using System.Collections.Generic;
using System.Reflection;

namespace MediatR.Internal;

internal static class TypeExtensions
{
    /// <summary>Gets types stack from which the provided <paramref name="type"/> inherits explicitly of implicitly.</summary>
    /// <param name="type">Type to query information from.</param>
    /// <param name="filter">The delegate that compares type against <paramref name="filterCriteria"/>.</param>
    /// <param name="filterCriteria">The search criteria that determines whether an type should be included in the returned collection.</param>
    /// <exception cref="ArgumentNullException">Either <paramref name="type"/> of <paramref name="filter"/> is null.</exception>
    /// <returns>Collection of types the provided <paramref name="type"/> inherits explicitly or implicitly.</returns>
    public static IEnumerable<Type> GetBaseTypes(this Type type, TypeFilter filter, object? filterCriteria)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (filter == null) throw new ArgumentNullException(nameof(filter));

        var collection = new Stack<Type>();
        while (true)
        {
            var i = type.GetInterfaces();
            var baseType = type.BaseType;
            if (baseType == null) return collection;

            if (filter(baseType, filterCriteria)) collection.Push(baseType);
            type = baseType;
        }
    }

    /// <summary>Gets types stack from which the provided <paramref name="type"/> inherits explicitly of implicitly.</summary>
    /// <param name="type">Type to query information from.</param>
    /// <returns>Collection of types the provided <paramref name="type"/> inherits explicitly or implicitly.</returns>
    public static IEnumerable<Type> GetBaseTypes(this Type type) => GetBaseTypes(type, static (t, _) => true, null);
}