using System;
using System.Collections.Generic;
using System.Reflection;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal struct TypeWrapper
{
    public static TypeWrapper Create(Type type, HashSet<Assembly> assembliesToScan, Dictionary<Type, TypeWrapper> typesToScanCache)
    {
        if (typesToScanCache.TryGetValue(type, out var value))
        {
            return value;
        }

        typesToScanCache[type] = value = new TypeWrapper(type);

        if (type.BaseType is not null && type != typeof(object) && assembliesToScan.Contains(type.BaseType!.Assembly))
        {
            var baseWrapper = Create(type.BaseType, assembliesToScan, typesToScanCache);
            value.SetBaseType(in baseWrapper);
        }

        return value;
    }

    private static int OpenGenericInterfaceComparer((Type Interface, Type) x, (Type Interface, Type) y) =>
        x.Interface.GetGenericArguments().Length.CompareTo(y.Interface.GetGenericArguments().Length);

    public bool IsOpenGeneric { get; }
    public Type[] GenericReplaceableTypeArguments { get; } = Array.Empty<Type>();
    public Type Type { get; }
    public TypeWrapper? BaseType { get; private set; }
    public List<TypeWrapper> TypesInheritingThisType { get; } = new();
    public Type[] Interfaces { get; }
    public (Type Interface, Type OpenGenericInterface)[] OpenGenericInterfaces { get; }

    public TypeWrapper(Type type)
    {
        Type = type;
        IsOpenGeneric = type.IsGenericTypeDefinition || type.ContainsGenericParameters;

        // GetInterfaces returns all interfaces implemented from the type hierarchy.
        Interfaces = type.GetInterfaces();

        var genericInterfaces = Array.FindAll(Interfaces,static t => t.ContainsGenericParameters);
        var openGenericInterfaces = Array.ConvertAll(genericInterfaces, t => (t, t.GetGenericTypeDefinition()));
        Array.Sort(openGenericInterfaces, OpenGenericInterfaceComparer);
        OpenGenericInterfaces = openGenericInterfaces;

        if (IsOpenGeneric)
        {
            GenericReplaceableTypeArguments = Array.FindAll(type.GetGenericArguments(), static t => t.IsGenericParameter);
        }
    }

    private void SetBaseType(in TypeWrapper typeWrapper)
    {
        BaseType = typeWrapper;
        typeWrapper.TypesInheritingThisType.Add(this);
    }
}