using System;
using System.Collections.Generic;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal sealed class TypeWrapper
{
    public static TypeWrapper Create(Type type, HashSet<AssemblyConfiguration> assembliesToScan, Dictionary<Type, TypeWrapper> typesToScanCache)
    {
        if (typesToScanCache.TryGetValue(type, out var typeWrapper))
        {
            return typeWrapper;
        }

        typesToScanCache[type] = typeWrapper = new TypeWrapper(type);

        if (type.BaseType is not null && type.BaseType != typeof(object) && assembliesToScan.Contains(new AssemblyConfiguration(type.BaseType.Assembly, default)))
        {
            var baseWrapper = Create(type.BaseType, assembliesToScan, typesToScanCache);
            typeWrapper.SetBaseType(in baseWrapper);
        }

        return typeWrapper;
    }

    public bool IsOpenGeneric { get; }
    public Type Type { get; }
    public TypeWrapper? BaseType { get; private set; }
    public List<TypeWrapper> TypesInheritingThisType { get; } = new();
    public Type[] Interfaces { get; }
    public (Type Interface, Type OpenGenericInterface)[] OpenGenericInterfaces { get; }

    public TypeWrapper(Type type)
    {
        Type = type;
        IsOpenGeneric = type.IsOpenGeneric();

        // GetInterfaces returns all interfaces implemented from the type hierarchy.
        Interfaces = type.GetInterfaces();

        var genericInterfaces = Array.FindAll(Interfaces, static t => t.IsGenericType || t.IsOpenGeneric());
        Array.Sort(genericInterfaces, static (x, y) => x.GetGenericArguments().Length.CompareTo(y.GetGenericArguments().Length));
        OpenGenericInterfaces = Array.ConvertAll(genericInterfaces, static t => (t, t.GetGenericTypeDefinition()));
    }

    private void SetBaseType(in TypeWrapper typeWrapper)
    {
        BaseType = typeWrapper;
        typeWrapper.TypesInheritingThisType.Add(this);
    }

    public override string ToString() => $"{nameof(TypeWrapper)} {{{Type}}}";
}