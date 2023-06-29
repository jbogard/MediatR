using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MediatR.DependencyInjection;

internal partial struct AssemblyScanner<TRegistrar>
{
    private sealed class TypeWrapper
    {
        public static TypeWrapper? Create(Type? type, List<Assembly> assembliesToScan, Dictionary<Type, TypeWrapper> typesToScanCache)
        {
            if (type is null || type == ObjectType)
            {
                return null;
            }

            if (typesToScanCache.TryGetValue(type, out var value))
            {
                return value;
            }

            typesToScanCache[type] = value = new TypeWrapper(type);

            if (value.Type.BaseType is not null && assembliesToScan.Contains(value.Type.BaseType.Assembly))
            {
                value.SetBaseType(Create(value.Type.BaseType, assembliesToScan, typesToScanCache));
            }

            return value;
        }

        public bool IsOpenGeneric { get; }
        public Type Type { get; }
        public TypeWrapper? BaseType { get; private set; }
        public List<TypeWrapper> TypesInheritingThisType { get; }
        public Type[] Interfaces { get; }
        public Type[] OpenGenericInterfaces { get; }

        private TypeWrapper(Type type)
        {
            Type = type;
            IsOpenGeneric = type.IsGenericTypeDefinition || type.ContainsGenericParameters;

            // GetInterfaces returns all interfaces implemented from the type hierarchy.
            Interfaces = type.GetInterfaces();
            OpenGenericInterfaces = Interfaces.Where(static t => t.ContainsGenericParameters).Select(static t => t.GetGenericTypeDefinition()).ToArray();
            TypesInheritingThisType = new List<TypeWrapper>();
        }

        private void SetBaseType(TypeWrapper? typeWrapper)
        {
            BaseType = typeWrapper;
            typeWrapper?.TypesInheritingThisType.Add(this);
        }
    }
}