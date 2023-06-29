using System;
using System.Collections.Generic;

namespace MediatR.DependencyInjection;

/// <summary>
/// Declares the DiC Registrar Adapter
/// </summary>
/// <typeparam name="TRegistrar">The Registrar type.</typeparam>
public sealed class DependencyInjectionRegistrarAdapter<TRegistrar>
{
    /// <summary>
    /// Get the registrar to setup MediatR.
    /// </summary>
    public TRegistrar Registrar { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a type to it self as a singleton and makes sure it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type> RegisterSelfSingletonOnlyOnce { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a open generic type to it self as a singleton and makes sure it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type> RegisterSelfOpenGenericSingletonOnlyOnce { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a singleton and makes sure it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterSingletonOnlyOnce { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its instance already created.
    /// </summary>
    public Action<TRegistrar, Type, object> RegisterInstance { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterMapping { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type and makes sure that its mapping is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterMappingOnlyOnce { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type with its implementing open generic type already registered but should be forwarded to this open generic service type.
    /// </summary>
    public Action<TRegistrar, Type, Type[]> RegisterOpenGenericMapping { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type with its implementing open generic type already registered but should be forwarded to this open generic service type and makes sure that its mapping is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type[]> RegisterOpenGenericMappingOnlyOnce { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterTransient { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type to the open generic implementation type as a transient.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterOpenGenericTransient { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient and makes sure that it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterTransientOnlyOnce { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type to the open generic implementation type as a transient and makes sure that it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterOpenGenericTransientOnlyOnce { get; }
    
    /// <summary>
    /// Creates an adapter to register all MediatR Services.
    /// </summary>
    /// <param name="registrar">The registrar to setup MediatR.</param>
    /// <param name="registerSelfSingletonOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a type to it self as a singleton and makes sure it is only registered once.</param>
    /// <param name="registerSelfOpenGenericSingletonOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a open generic type to it self as a singleton and makes sure it is only registered once.</param>
    /// <param name="registerSingletonOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a singleton and makes sure it is only registered once.</param>
    /// <param name="registerInstance">Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its instance already created.</param>
    /// <param name="registerMapping">Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type.</param>
    /// <param name="registerMappingOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type and makes sure that its mapping is only registered once.</param>
    /// <param name="registerOpenGenericMapping">Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type with its implementing open generic type already registered but should be forwarded to this open generic service type.</param>
    /// <param name="registerOpenGenericMappingOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type with its implementing open generic type already registered but should be forwarded to this open generic service type and makes sure that its mapping is only registered once.</param>
    /// <param name="registerTransient">Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient.</param>
    /// <param name="registerOpenGenericTransient">Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type to the open generic implementation type as a transient.</param>
    /// <param name="registerTransientOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient and makes sure that it is only registered once.</param>
    /// <param name="registerOpenGenericTransientOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a open generic service type to the open generic implementation type as a transient and makes sure that it is only registered once.</param>
    public DependencyInjectionRegistrarAdapter(
        TRegistrar registrar,
        Action<TRegistrar, Type, Type> registerSingletonOnlyOnce,
        Action<TRegistrar, Type> registerSelfSingletonOnlyOnce,
        Action<TRegistrar, Type> registerSelfOpenGenericSingletonOnlyOnce,
        Action<TRegistrar, Type, object> registerInstance,
        Action<TRegistrar, Type, Type> registerMapping,
        Action<TRegistrar, Type, Type> registerMappingOnlyOnce,
        Action<TRegistrar, Type, Type[]> registerOpenGenericMapping,
        Action<TRegistrar, Type, Type[]> registerOpenGenericMappingOnlyOnce,
        Action<TRegistrar, Type, Type> registerTransient,
        Action<TRegistrar, Type, Type> registerOpenGenericTransient,
        Action<TRegistrar, Type, Type> registerTransientOnlyOnce,
        Action<TRegistrar, Type, Type> registerOpenGenericTransientOnlyOnce)
    {
        Registrar = registrar;
        RegisterSelfSingletonOnlyOnce = registerSelfSingletonOnlyOnce;
        RegisterSelfOpenGenericSingletonOnlyOnce = registerSelfOpenGenericSingletonOnlyOnce;
        RegisterSingletonOnlyOnce = registerSingletonOnlyOnce;
        RegisterInstance = registerInstance;
        RegisterMapping = registerMapping;
        RegisterMappingOnlyOnce = registerMappingOnlyOnce;
        RegisterOpenGenericMapping = registerOpenGenericMapping;
        RegisterOpenGenericMappingOnlyOnce = registerOpenGenericMappingOnlyOnce;
        RegisterTransient = registerTransient;
        RegisterOpenGenericTransient = registerOpenGenericTransient;
        RegisterTransientOnlyOnce = registerTransientOnlyOnce;
        RegisterOpenGenericTransientOnlyOnce = registerOpenGenericTransientOnlyOnce;
    }

    internal void Register(
        MediatRServiceConfiguration<TRegistrar> configuration,
        Type implementingType,
        IEnumerable<Type> serviceTypes,
        bool mustOnlyRegisterOnce)
    {
        if (configuration.RegistrationOptions == RegistrationOptions.Transient)
        {
            foreach (var serviceType in serviceTypes)
            {
                if (mustOnlyRegisterOnce)
                {
                    RegisterTransientOnlyOnce(Registrar, serviceType, implementingType);
                }
                else
                {
                    RegisterTransient(Registrar, serviceType, implementingType);
                }
            }
        }
        else
        {
            RegisterSelfSingletonOnlyOnce(Registrar, implementingType);
            foreach (var serviceType in serviceTypes)
            {
                if (mustOnlyRegisterOnce)
                {
                    RegisterMappingOnlyOnce(Registrar, implementingType, serviceType);
                }
                else
                {
                    RegisterMapping(Registrar, implementingType, serviceType);
                }
            }
        }
    }

    internal void RegisterSingleton(Type implementationType, IEnumerable<Type> serviceTypes)
    {
        RegisterSelfSingletonOnlyOnce(Registrar, implementationType);
        foreach (var serviceType in serviceTypes)
        {
            RegisterMappingOnlyOnce(Registrar, implementationType, serviceType);
        }
    }
}