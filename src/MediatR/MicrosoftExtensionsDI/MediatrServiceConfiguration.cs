using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using MediatR.NotificationPublishers;

namespace Microsoft.Extensions.DependencyInjection;

public class MediatRServiceConfiguration
{
    /// <summary>
    /// Optional filter for types to register. Default value is a function returning true.
    /// </summary>
    public Func<Type, bool> TypeEvaluator { get; set; } = t => true;
    
    /// <summary>
    /// Mediator implementation type to register. Default is <see cref="Mediator"/>
    /// </summary>
    public Type MediatorImplementationType { get; set; } = typeof(Mediator);

    /// <summary>
    /// Strategy for publishing notifications. Defaults to <see cref="ForeachAwaitPublisher"/>
    /// </summary>
    public INotificationPublisher NotificationPublisher { get; set; } = new ForeachAwaitPublisher();

    /// <summary>
    /// Type of notification publisher strategy to register. If set, overrides <see cref="NotificationPublisher"/>
    /// </summary>
    public Type? NotificationPublisherType { get; set; }

    /// <summary>
    /// Service lifetime to register services under. Default value is <see cref="ServiceLifetime.Transient"/>
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Request exception action processor strategy. Default value is <see cref="DependencyInjection.RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions"/>
    /// </summary>
    public RequestExceptionActionProcessorStrategy RequestExceptionActionProcessorStrategy { get; set; }
        = RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions;

    internal List<Assembly> AssembliesToRegister { get; } = new();

    /// <summary>
    /// List of behaviors to register in specific order
    /// </summary>
    public List<ServiceDescriptor> BehaviorsToRegister { get; } = new();

    /// <summary>
    /// Register various handlers from assembly containing given type
    /// </summary>
    /// <typeparam name="T">Type from assembly to scan</typeparam>
    /// <returns>This</returns>
    public MediatRServiceConfiguration RegisterServicesFromAssemblyContaining<T>()
        => RegisterServicesFromAssemblyContaining(typeof(T));

    /// <summary>
    /// Register various handlers from assembly containing given type
    /// </summary>
    /// <param name="type">Type from assembly to scan</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration RegisterServicesFromAssemblyContaining(Type type)
        => RegisterServicesFromAssembly(type.Assembly);

    /// <summary>
    /// Register various handlers from assembly
    /// </summary>
    /// <param name="assembly">Assembly to scan</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);

        return this;
    }

    /// <summary>
    /// Register various handlers from assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration RegisterServicesFromAssemblies(
        params Assembly[] assemblies)
    {
        AssembliesToRegister.AddRange(assemblies);

        return this;
    }

    /// <summary>
    /// Register a closed behavior type
    /// </summary>
    /// <typeparam name="TServiceType">Closed behavior interface type</typeparam>
    /// <typeparam name="TImplementationType">Closed behavior implementation type</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddBehavior<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddBehavior(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed behavior type
    /// </summary>
    /// <param name="serviceType">Closed behavior interface type</param>
    /// <param name="implementationType">Closed behavior implementation type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddBehavior(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        BehaviorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));

        return this;
    }

    /// <summary>
    /// Registers an open behavior type against the <see cref="IPipelineBehavior{TRequest,TResponse}"/> open generic interface type
    /// </summary>
    /// <param name="openBehaviorType">An open generic behavior type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddOpenBehavior(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericType)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be generic");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IPipelineBehavior<,>) || i == typeof(IStreamPipelineBehavior<,>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IPipelineBehavior<,>).FullName} or {typeof(IStreamPipelineBehavior<,>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            BehaviorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }
}