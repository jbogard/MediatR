using System;
using MediatR.DependencyInjection.Configuration;

namespace MediatR.DependencyInjection;

/// <summary>
/// Declares the DiC Registrar Adapter.
/// </summary>
/// <typeparam name="TRegistrar">The Registrar type.</typeparam>
/// <typeparam name="TConfiguration">The MediarR configuration.</typeparam>
public abstract class DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration>
    where TConfiguration : MediatRServiceConfiguration
{
    /// <summary>
    /// Get the registrar where to setup MediatR.
    /// </summary>
    protected TRegistrar Registrar { get; }

    /// <summary>
    /// Gets the current used configuration for MediatR.
    /// </summary>
    protected TConfiguration Configuration { get; }

    /// <summary>
    /// Creates an instance of <see cref="DependencyInjectionRegistrarAdapter{TRegistrar,TConfiguration}"/>
    /// </summary>
    /// <param name="registrar">The registrar where to register MediatR.</param>
    /// <param name="configuration">The configuration to use to register MediatR.</param>
    protected DependencyInjectionRegistrarAdapter(TRegistrar registrar, TConfiguration configuration)
    {
        Registrar = registrar;
        Configuration = configuration;
    }

    /// <summary>
    /// Registers a type to it self as a single instance.
    /// </summary>
    /// <param name="serviceType">The service and implementation type.</param>
    public virtual void RegisterSelfSingleton(Type serviceType) =>
        RegisterSingleton(serviceType, serviceType);

    /// <summary>
    /// Registers an open generic type to it self as a single instance.
    /// </summary>
    /// <param name="serviceType">The open generic service and implementation type.</param>
    public virtual void RegisterOpenGenericSelfSingleton(Type serviceType) =>
        RegisterOpenGenericSingleton(serviceType, serviceType);

    /// <summary>
    /// Registers a service type to its instance.
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="instance"></param>
    public abstract void RegisterInstance(Type serviceType, object instance);

    /// <summary>
    /// Registers the <paramref name="serviceType"/> to the <paramref name="implementationType"/> as a singleton.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementing type.</param>
    public abstract void RegisterSingleton(Type serviceType, Type implementationType);

    /// <summary>
    /// Registers the open generic <paramref name="serviceType"/> to the open generic <paramref name="implementationType"/> as a singleton.
    /// </summary>
    /// <param name="serviceType">The open generic service type.</param>
    /// <param name="implementationType">The open generic implementation type.</param>
    public abstract void RegisterOpenGenericSingleton(Type serviceType, Type implementationType);

    /// <summary>
    /// Registers a mapping or a factory to get for the service type the implementation type.
    /// </summary>
    /// <remarks>
    /// This is to ensure that a single implementation type can have multiple service types but only creates one instance.
    /// </remarks>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    public abstract void RegisterMapping(Type serviceType, Type implementationType);

    /// <summary>
    /// Registers a mapping or a factory to get for the open generic service type the open generic implementation type.
    /// </summary>
    /// <remarks>
    /// This is to ensure that a single open generic implementation type can have multiple open generic service types but only creates one instance.
    /// </remarks>
    /// <param name="serviceType">The open generic service type.</param>
    /// <param name="implementationType">The open generic implementation type.</param>
    public abstract void RegisterOpenGenericMapping(Type serviceType, Type implementationType);

    /// <summary>
    /// Registers a mapping or a factory to get for the service type the implementation type but ensures that it exists only once.
    /// </summary>
    /// <remarks>
    /// This is to ensure that a single implementation type can have multiple service types but only creates one instance.
    /// </remarks>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    public virtual void RegisterMappingOnlyOnce(Type serviceType, Type implementationType)
    {
        if (!IsAlreadyRegistered(serviceType, implementationType))
        {
            RegisterMapping(serviceType, implementationType);
        }
    }

    /// <summary>
    /// Registers a mapping or a factory to get for the open generic service type the open generic implementation type but ensures that it exists only once.
    /// </summary>
    /// <remarks>
    /// This is to ensure that a single open generic implementation type can have multiple open generic service types but only creates one instance.
    /// </remarks>
    /// <param name="serviceType">The open generic service type.</param>
    /// <param name="implementationType">The open generic implementation type.</param>
    public virtual void RegisterOpenGenericMappingOnlyOnce(Type serviceType, Type implementationType)
    {
        if (!IsAlreadyRegistered(serviceType, implementationType))
        {
            RegisterOpenGenericMapping(serviceType, implementationType);
        }
    }

    /// <summary>
    /// Registers the <paramref name="serviceType"/> with the <paramref name="implementationType"/> with a configurable service lifetime.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    public abstract void Register(Type serviceType, Type implementationType);

    /// <summary>
    /// Registers the open generic <paramref name="serviceType"/> with the open generic <paramref name="implementationType"/> with a configurable service lifetime.
    /// </summary>
    /// <param name="serviceType">The open generic service type.</param>
    /// <param name="implementationType">The open generic implementation type.</param>
    public abstract void RegisterOpenGeneric(Type serviceType, Type implementationType);

    /// <summary>
    /// Registers the <paramref name="serviceType"/> with the <paramref name="implementationType"/> with a configurable service lifetime but ensures that it exists only once.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    public virtual void RegisterOnlyOnce(Type serviceType, Type implementationType)
    {
        if (!IsAlreadyRegistered(serviceType, implementationType))
        {
            Register(serviceType, implementationType);
        }
    }

    /// <summary>
    /// Registers the open generic <paramref name="serviceType"/> with the open generic <paramref name="implementationType"/> with a configurable service lifetime but ensures that it exists only once.
    /// </summary>
    /// <param name="serviceType">The open generic service type.</param>
    /// <param name="implementationType">The open generic implementation type.</param>
    public virtual void RegisterOpenGenericOnlyOnce(Type serviceType, Type implementationType)
    {
        if (!IsAlreadyRegistered(serviceType, implementationType))
        {
            RegisterOpenGeneric(serviceType, implementationType);
        }
    }

    /// <summary>
    /// Checks if a service type is already registered with the same implementation type.
    /// </summary>
    /// <remarks>
    /// This also includes open generic services and not open generic services.
    /// </remarks>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <returns><c>true</c> if the service with its implementation type is already registered in the container; else <c>false</c>.</returns>
    public abstract bool IsAlreadyRegistered(Type serviceType, Type implementationType);
}