using System;
using System.Runtime.CompilerServices;

namespace MediatR.DependencyInjection;

internal static class TypeExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOpenGeneric(this Type type) => type.ContainsGenericParameters;
}