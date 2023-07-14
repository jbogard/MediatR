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
    private static MediatRMicrosoftDicConfiguration config = null!;
    
    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="serviceCollection">Service collection</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection ConfigureMediatR(this IServiceCollection serviceCollection, Action<MediatRMicrosoftDicConfiguration> configuration)
    {
        var adapter = new DependencyInjectionRegistrarAdapter<IServiceCollection>(
            serviceCollection,
            RegisterSingletonOnlyOnce,
            RegisterSelfSingleton,
            RegisterInstance,
            RegisterMapping,
            RegisterMappingOnlyOnce,
            RegisterTransient,
            RegisterTransientOnlyOnce
        );
        
        config = new MediatRMicrosoftDicConfiguration(adapter);
        
        configuration(config);
        
        MediatRConfigurator.ConfigureMediatR(config);

        return serviceCollection;
    }
    
    private static void RegisterSingletonOnlyOnce(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.TryAddEnumerable(new ServiceDescriptor(implementingType, serviceType, ServiceLifetime.Singleton));
    private static void RegisterSelfSingleton(IServiceCollection serviceCollection, Type serviceType) =>
        serviceCollection.AddSingleton(serviceType, serviceType);
    private static void RegisterInstance(IServiceCollection serviceCollection, Type serviceType, object instance) =>
        serviceCollection.AddSingleton(serviceType, instance);
    private static void RegisterMapping(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.Add(new ServiceDescriptor(serviceType, sp => sp.GetService(implementingType), config.MappingServiceLifetime));
    private static void RegisterMappingOnlyOnce(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.TryAddEnumerable(implementingType, new ServiceDescriptor(serviceType, sp => sp.GetService(implementingType), config.MappingServiceLifetime));
    private static void RegisterTransient(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.AddTransient(serviceType, implementingType);
    private static void RegisterTransientOnlyOnce(IServiceCollection serviceCollection, Type serviceType, Type implementingType) =>
        serviceCollection.TryAddTransient(serviceType, implementingType);

    private static void TryAddEnumerable(this IServiceCollection serviceCollection, Type implementingType, ServiceDescriptor serviceDescriptor)
    {
        for (var i = 0; i < serviceCollection.Count; i++)
        {
            var existingDescriptor = serviceCollection[i];
            if (existingDescriptor.ServiceType == serviceDescriptor.ServiceType && existingDescriptor.GetImplementationType() == implementingType)
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