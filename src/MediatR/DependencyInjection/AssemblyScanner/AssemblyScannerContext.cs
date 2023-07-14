using System;
using System.Collections.Generic;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal readonly ref struct AssemblyScannerContext<TRegistrar>
{
    public IComparer<Type> TypeComparerInstance { get; } = new TypeComparer();
    public IComparer<Type> GenericReplaceableTypeComparerInstance { get; } = new GenericReplaceableTypeComparer();

    public TypeWrapper[] TypesToScan { get; }
    public Dictionary<Type, HashSet<TypeWrapper>> MessageMapping { get; } = new();
    public Dictionary<Type, HashSet<TypeWrapper>>.KeyCollection MessageTypes => MessageMapping.Keys;

    public MediatRServiceConfiguration<TRegistrar> Configuration { get; }
    
    public AssemblyScannerContext(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        Configuration = configuration;
        var typeComparer = TypeComparerInstance;

        var typeToScanCache = new Dictionary<Type, TypeWrapper>();

        foreach (var assembly in configuration.AssembliesToRegister)
        {
            foreach (var definedType in assembly.DefinedTypes)
            {
                if (!definedType.IsEnum && !definedType.IsInterface && configuration.TypeEvaluator(definedType))
                {
                    var typeWrapper = TypeWrapper.Create(definedType, configuration.AssembliesToRegister, typeToScanCache);
                    ArrayBuilder<TypeWrapper>.Instance.Add(typeWrapper);
                }
            }
        }

        TypesToScan = ArrayBuilder<TypeWrapper>.Instance.ToArray();

        var notifications = new HashSet<TypeWrapper>(Array.FindAll(TypesToScan, wrapper => Array.BinarySearch(wrapper.Interfaces, typeof(INotification), typeComparer) >= 0));
        MessageMapping.Add(typeof(INotification), notifications);
        var requests = new HashSet<TypeWrapper>(Array.FindAll(TypesToScan, wrapper => Array.BinarySearch(wrapper.Interfaces, typeof(IRequest), typeComparer) >= 0));
        MessageMapping.Add(typeof(IRequest), requests);
        var requestResponses = new HashSet<TypeWrapper>(Array.FindAll(TypesToScan, wrapper => wrapper.IsOpenGeneric && Array.BinarySearch(wrapper.OpenGenericInterfaces, typeof(IRequest<>)) > 0));
        MessageMapping.Add(typeof(IRequest<>), requestResponses);
        var streamRequests = new HashSet<TypeWrapper>(Array.FindAll(TypesToScan, wrapper => wrapper.IsOpenGeneric && Array.BinarySearch(wrapper.OpenGenericInterfaces, typeof(IStreamRequest<>)) > 0));
        MessageMapping.Add(typeof(IStreamRequest<>), streamRequests);
    }

    private sealed class TypeComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y) => x.GUID.CompareTo(y.GUID);
    }

    private sealed class GenericReplaceableTypeComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y) => string.CompareOrdinal(x.Name, y.Name);
    }
}