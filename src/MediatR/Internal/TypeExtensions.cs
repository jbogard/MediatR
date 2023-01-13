using System;
using System.Collections.Generic;
using System.Reflection;

namespace MediatR.Internal;

internal static class TypeExtensions
{
    public static IEnumerable<Type> GetBaseTypes(this Type type, TypeFilter filter, object? filterCriteria)
    {
        var collection = new HashSet<Type>();

        return Recursion(collection, type, filter, filterCriteria);

        static ICollection<Type> Recursion(ICollection<Type> collection, Type currentType, TypeFilter filter, object? filterCriteria)
        {
            var baseType = currentType.BaseType;
            if (filter(baseType, filterCriteria))
            {
                collection.Add(baseType);
                Recursion(collection, baseType, filter, filterCriteria);
            }

            return collection;
        }
    }

    public static IEnumerable<Type> GetBaseTypes(this Type type) => GetBaseTypes(type, static (t, _) => true, null);
}