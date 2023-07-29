using System;
using System.Collections.Generic;
using System.Linq;

namespace MediatR.DependencyInjection.Configuration;

/// <summary>
/// Defines a registrar for a specific open generic type to be registered.
/// </summary>
public sealed class TypeRegistrar
{
    private readonly Type _openGenericInterface;
    private readonly bool _mustBeSingleRegistration;

    internal HashSet<(Type ServiceType, Type implementationType)> Services { get; } = new();

    internal TypeRegistrar(Type openGenericInterface, bool mustBeSingleRegistration)
    {
        _openGenericInterface = openGenericInterface;
        _mustBeSingleRegistration = mustBeSingleRegistration;
    }

    /// <summary>
    /// Register a closed type.
    /// </summary>
    /// <typeparam name="TServiceType">Closed interface type</typeparam>
    /// <typeparam name="TImplementationType">Closed implementation type</typeparam>
    /// <returns>This</returns>
    public TypeRegistrar Add<TServiceType, TImplementationType>()
        where TImplementationType : TServiceType =>
        Add(typeof(TServiceType), typeof(TImplementationType));

    /// <summary>
    /// Register a closed generic type against its implementations
    /// </summary>
    /// <typeparam name="TImplementationType">Closed implementation type</typeparam>
    /// <returns>This</returns>
    public TypeRegistrar Add<TImplementationType>() =>
        AddOpenGeneric(typeof(TImplementationType));

    /// <summary>
    /// Register a closed type.
    /// </summary>
    /// <param name="serviceType">Closed interface type</param>
    /// <param name="implementationType">Closed implementation type</param>
    /// <returns>This</returns>
    public TypeRegistrar Add(Type serviceType, Type implementationType)
    {
        if (serviceType.GetGenericTypeDefinition() != _openGenericInterface || !serviceType.IsAssignableFrom(implementationType))
        {
            throw new ArgumentException($"Service Type '{serviceType}' or implementing Type '{implementationType}' must inherit from any covariant of Type '{_openGenericInterface}' to be registered.");
        }

        Services.Add((serviceType, implementationType));
        return this;
    }

    /// <summary>
    /// Registers the <paramref name="openGenericType"/> implementation type.
    /// </summary>
    /// <param name="openGenericType">The open generic implementation type</param>
    /// <returns>This</returns>
    public TypeRegistrar AddOpenGeneric(Type openGenericType)
    {
        var implementedGenericInterfaces = openGenericType
            .GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == _openGenericInterface)
            .ToArray();

        if (implementedGenericInterfaces.Length == 0)
        {
            throw new ArgumentException($"Type '{openGenericType}' must implement '{_openGenericInterface}' to be registered.");
        }

        foreach (var implementedBehaviorType in implementedGenericInterfaces)
        {
            Add(implementedBehaviorType, openGenericType);
        }

        return this;
    }

    internal void Register<TRegistrar, TConfiguration>(
        DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter,
        MediatRServiceConfiguration configuration)
        where TConfiguration : MediatRServiceConfiguration
    {
        foreach (var grouping in Services.GroupBy(t => t.implementationType))
        {
            var serviceTypes = grouping
                .Select(kvp => (kvp.ServiceType, _mustBeSingleRegistration))
                .ToArray();

            if (grouping.Key.IsOpenGeneric())
            {
                adapter.RegisterOpenGeneric(configuration, grouping.Key, serviceTypes);
            }
            else
            {
                adapter.Register(configuration, grouping.Key, serviceTypes);
            }
        }
    }

    internal bool ContainsRegistration(Type serviceType, Type implementationType) =>
        Services.Contains((serviceType, implementationType));
}