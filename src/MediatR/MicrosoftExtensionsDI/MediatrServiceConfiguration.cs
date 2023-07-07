using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using MediatR.NotificationPublishers;
using MediatR.Pipeline;

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
    /// List of stream behaviors to register in specific order
    /// </summary>
    public List<ServiceDescriptor> StreamBehaviorsToRegister { get; } = new();

    /// <summary>
    /// List of request pre processors to register in specific order
    /// </summary>
    public List<ServiceDescriptor> RequestPreProcessorsToRegister { get; } = new();

    /// <summary>
    /// List of request post processors to register in specific order
    /// </summary>
    public List<ServiceDescriptor> RequestPostProcessorsToRegister { get; } = new();

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
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IPipelineBehavior<,>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IPipelineBehavior<,>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            BehaviorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Register a closed stream behavior type
    /// </summary>
    /// <typeparam name="TServiceType">Closed stream behavior interface type</typeparam>
    /// <typeparam name="TImplementationType">Closed stream behavior implementation type</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddStreamBehavior<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddStreamBehavior(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);
    
    /// <summary>
    /// Register a closed stream behavior type
    /// </summary>
    /// <param name="serviceType">Closed stream behavior interface type</param>
    /// <param name="implementationType">Closed stream behavior implementation type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddStreamBehavior(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        StreamBehaviorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));

        return this;
    }
    
    /// <summary>
    /// Registers an open stream behavior type against the <see cref="IStreamPipelineBehavior{TRequest,TResponse}"/> open generic interface type
    /// </summary>
    /// <param name="openBehaviorType">An open generic stream behavior type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddOpenStreamBehavior(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericType)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be generic");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IStreamPipelineBehavior<,>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IStreamPipelineBehavior<,>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            StreamBehaviorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }


    /// <summary>
    /// Register a closed request pre processor type
    /// </summary>
    /// <typeparam name="TServiceType">Closed request pre processor interface type</typeparam>
    /// <typeparam name="TImplementationType">Closed request pre processor implementation type</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddRequestPreProcessor<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddRequestPreProcessor(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);
    
    /// <summary>
    /// Register a closed request pre processor type
    /// </summary>
    /// <param name="serviceType">Closed request pre processor interface type</param>
    /// <param name="implementationType">Closed request pre processor implementation type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddRequestPreProcessor(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        RequestPreProcessorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));

        return this;
    }
    
    /// <summary>
    /// Registers an open request pre processor type against the <see cref="IRequestPreProcessor{TRequest}"/> open generic interface type
    /// </summary>
    /// <param name="openBehaviorType">An open generic request pre processor type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddOpenRequestPreProcessor(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericType)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be generic");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IRequestPreProcessor<>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IRequestPreProcessor<>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            RequestPreProcessorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }
    
    /// <summary>
    /// Register a closed request post processor type
    /// </summary>
    /// <typeparam name="TServiceType">Closed request post processor interface type</typeparam>
    /// <typeparam name="TImplementationType">Closed request post processor implementation type</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddRequestPostProcessor<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddRequestPreProcessor(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);
    
    /// <summary>
    /// Register a closed request post processor type
    /// </summary>
    /// <param name="serviceType">Closed request post processor interface type</param>
    /// <param name="implementationType">Closed request post processor implementation type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddRequestPostProcessor(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        RequestPostProcessorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));

        return this;
    }
    
    /// <summary>
    /// Registers an open request post processor type against the <see cref="IRequestPostProcessor{TRequest,TResponse}"/> open generic interface type
    /// </summary>
    /// <param name="openBehaviorType">An open generic request post processor type</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public MediatRServiceConfiguration AddOpenRequestPostProcessor(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericType)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be generic");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IRequestPostProcessor<,>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IRequestPostProcessor<,>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            RequestPostProcessorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }


}