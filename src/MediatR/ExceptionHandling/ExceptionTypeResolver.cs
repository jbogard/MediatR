using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MediatR.ExceptionHandling;

internal static class ExceptionTypeResolver
{
    private static readonly Type ObjectType = typeof(object);

    // Mapped the exception type with itself and the type hierarchy of the exception type.
    private static readonly ConcurrentDictionary<Type, Type[]> exceptionTypeHierarchy = new();

    [ThreadStatic]
    private static List<Type>? arrayBuilder;

    public static Type[] GetExceptionTypeHierarchy(Type rootExceptionType) =>
        exceptionTypeHierarchy.GetOrAdd(rootExceptionType, ExceptionHierarchyFactory);

    private static Type[] ExceptionHierarchyFactory(Type? rootExceptionType)
    {
        arrayBuilder ??= new List<Type>();

        while (rootExceptionType is not null && rootExceptionType != ObjectType)
        {
            arrayBuilder.Add(rootExceptionType);
            rootExceptionType = rootExceptionType.BaseType;
        }

        var result = arrayBuilder.ToArray();
        arrayBuilder.Clear();
        return result;
    }
}