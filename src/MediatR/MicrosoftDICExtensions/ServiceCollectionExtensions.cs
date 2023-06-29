using System;
using System.Reflection;
using MediatR.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.MicrosoftDICExtensions;

/// <summary>
/// Registers MediatR in a ServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="serviceCollection">Service collection</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection ConfigureMediatR(this IServiceCollection serviceCollection, Action<MediatRServiceConfiguration<IServiceCollection>> configuration)
    {
        var adapter = new DependencyInjectionRegistrarAdapter<IServiceCollection>(
            serviceCollection,
            SingletonOnlyOnce,
            SelfSingletonOnlyOnce,
            SelfOpenGenericSingletonOnlyOnce,
            Instance,
            Mapping,
            MappingOnlyOnce,
            OpenGenericMapping,
            OpenGenericMappingOnlyOnce,
            Transient,
            TransientOpenGeneric,
            TransientOnlyOnce,
            TransientOpenGenericOnlyOnce);

        var config = new MediatRServiceConfiguration<IServiceCollection>(adapter);

        configuration(config);
        
        MediatRConfigurator.ConfigureMediatR(config);

        return serviceCollection;
    }

    private static void SingletonOnlyOnce(IServiceCollection serviceCollection, Type serviceType, Type implementationType) =>
        serviceCollection.TryAddEnumerable(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton));
    private static void SelfSingletonOnlyOnce(IServiceCollection serviceCollection, Type serviceType) =>
        serviceCollection.TryAddSingleton(serviceType);
    private static void SelfOpenGenericSingletonOnlyOnce(IServiceCollection serviceCollection, Type serviceType) =>
        serviceCollection.TryAddSingleton(serviceType);
    private static void Instance(IServiceCollection serviceCollection, Type serviceType, object instance) =>
        serviceCollection.AddSingleton(serviceType, instance);
    private static void Mapping(IServiceCollection serviceCollection, Type fromType, Type toType) =>
        serviceCollection.AddSingleton(toType, sp => sp.GetRequiredService(fromType));
    private static void MappingOnlyOnce(IServiceCollection serviceCollection, Type fromType, Type toType) =>
        serviceCollection.TryAddEnumerable(toType, fromType, new ServiceDescriptor(toType, sp => sp.GetRequiredService(fromType), ServiceLifetime.Singleton));
    private static void OpenGenericMapping(IServiceCollection serviceCollection, Type fromType, Type[] toTypes)
    {
        if (toTypes.Length is not 1)
        {
            throw new InvalidOperationException($"The type '{fromType}' has multiple open generic interfaces '{string.Join<Type>(", ", toTypes)}' which are not supported by service collection  to be registered as such.");
        }

        serviceCollection.AddSingleton(toTypes[0], fromType);
    }
    private static void OpenGenericMappingOnlyOnce(IServiceCollection serviceCollection, Type fromType, Type[] toType)
    {
        if (toType.Length is not 1)
        {
            throw new InvalidOperationException($"The type '{fromType}' has multiple open generic interfaces '{string.Join<Type>(", ", toType)}' which are not supported by service collection to be registered as such.");
        }

        serviceCollection.TryAddEnumerable(new ServiceDescriptor(toType[0], fromType, ServiceLifetime.Singleton));
    }
    private static void Transient(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.AddTransient(serviceType, implementingType);
    private static void TransientOnlyOnce(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.TryAddEnumerable(new ServiceDescriptor(serviceType, implementingType, ServiceLifetime.Transient));
    private static void TransientOpenGeneric(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.AddTransient(serviceType, implementingType);
    private static void TransientOpenGenericOnlyOnce(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.TryAddEnumerable(new ServiceDescriptor(serviceType, implementingType, ServiceLifetime.Transient));

    private static void TryAddEnumerable(this IServiceCollection serviceCollection, Type serviceType, Type implementingType, ServiceDescriptor serviceDescriptor)
    {
        for (var i = 0; i < serviceCollection.Count; i++)
        {
            var existingDescriptor = serviceCollection[i];
            if (existingDescriptor.ServiceType == serviceType && existingDescriptor.GetImplementationType() == implementingType)
            {
                return;
            }
        }

        serviceCollection.Add(serviceDescriptor);
    }

    private static Type? GetImplementationType(this ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.ImplementationType is not null)
        {
            return serviceDescriptor.ImplementationType;
        }

        if (serviceDescriptor.ImplementationInstance is not null)
        {
            return serviceDescriptor.ImplementationInstance.GetType();
        }

        if (serviceDescriptor.ImplementationFactory is not null)
        {
            var methodInfo = serviceDescriptor.ImplementationFactory.GetMethodInfo();
            return methodInfo.ReturnType == typeof(object) ? null : methodInfo.ReturnType;
        }

        return null;
    }
}