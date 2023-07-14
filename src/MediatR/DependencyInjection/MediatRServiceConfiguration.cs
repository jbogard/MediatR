using System;
using System.Collections.Generic;
using System.Reflection;
using MediatR.Abstraction;
using MediatR.Abstraction.Behaviors;
using MediatR.NotificationPublishers;

namespace MediatR.DependencyInjection;

public abstract class MediatRServiceConfiguration
{
    /// <summary>
    /// Optional filter for types to register. Default value is a function returning true.
    /// </summary>
    public Predicate<Type> TypeEvaluator { get; set; } = _ => true;

    /// <summary>
    /// Strategy for publishing notifications. Defaults to <see cref="ForeachAwaitPublisher"/>
    /// </summary>
    public INotificationPublisher NotificationPublisher { get; set; } = new ForeachAwaitPublisher();

    /// <summary>
    /// Type of notification publisher strategy to register. If set, overrides <see cref="NotificationPublisher"/>
    /// </summary>
    public Type? NotificationPublisherType { get; set; }

    /// <summary>
    /// Service lifetime to register services under. Default value is <see cref="RegistrationOptions.SingletonAndMapped"/>
    /// </summary>
    public RegistrationOptions RegistrationOptions { get; set; } = RegistrationOptions.SingletonAndMapped;

    /// <summary>
    /// Gets or sets a value indicating whenever the Assembly Scanner should throw on not supported open generic types. Default is <c>true</c>.
    /// <remarks>
    /// A not supported open generic type is when the open generic parameter number exceeds 3.
    /// </remarks>
    /// </summary>
    public bool ThrowOnNotSupportedOpenGenerics { get; set; } = true;

    /// <summary>
    /// Request exception action processor strategy. Default value is <see cref="DependencyInjection.RequestExceptionActionProcessorStrategy.ApplyForAllExceptions"/>
    /// </summary>
    public RequestExceptionActionProcessorStrategy RequestExceptionActionProcessorStrategy { get; set; } = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;

    public List<Type> ExceptionTypes { get; } = new()
    {
        typeof(Exception),
        typeof(AggregateException),
        typeof(ArgumentException),
        typeof(InvalidOperationException),
    };

    /// <summary>
    /// List of behaviors to register in specific order
    /// </summary>
    public Dictionary<Type, Type[]> BehaviorsToRegister { get; } = new();

    internal HashSet<Assembly> AssembliesToRegister { get; } = new();
    
    /// <summary>
    /// Register various handlers from assembly containing given type
    /// </summary>
    /// <typeparam name="T">Type from assembly to scan</typeparam>
    /// <returns>This</returns>
    public void RegisterServicesFromAssemblyContaining<T>() =>
        RegisterServicesFromAssemblyContaining(typeof(T));

    /// <summary>
    /// Register various handlers from assembly containing given type
    /// </summary>
    /// <param name="type">Type from assembly to scan</param>
    /// <returns>This</returns>
    public void RegisterServicesFromAssemblyContaining(Type type) =>
        RegisterServicesFromAssembly(type.Assembly);

    /// <summary>
    /// Register various handlers from assembly
    /// </summary>
    /// <param name="assembly">Assembly to scan</param>
    /// <returns>This</returns>
    public void RegisterServicesFromAssembly(Assembly assembly) =>
        AssembliesToRegister.Add(assembly);

    /// <summary>
    /// Register various handlers from assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>This</returns>
    public void RegisterServicesFromAssemblies(params Assembly[] assemblies) =>
        AssembliesToRegister.UnionWith(assemblies);

    /// <summary>
    /// Register a closed behavior type
    /// </summary>
    /// <typeparam name="TServiceType">Closed behavior interface type</typeparam>
    /// <typeparam name="TImplementationType">Closed behavior implementation type</typeparam>
    /// <returns>This</returns>
    public void AddBehavior<TImplementationType, TServiceType>()
        => AddBehavior(typeof(TImplementationType), typeof(TServiceType));

    /// <summary>
    /// Register a closed behavior type
    /// </summary>
    /// <param name="serviceType">Closed behavior interface type</param>
    /// <param name="implementationTypes">Closed behavior implementation type</param>
    /// <returns>This</returns>
    public void AddBehavior(Type serviceType, params Type[] implementationTypes) =>
        BehaviorsToRegister.Add(serviceType, implementationTypes);

    /// <summary>
    /// Registers an open behavior type against the <see cref="IPipelineBehavior{TRequest}"/> open generic interface type
    /// </summary>
    /// <param name="openBehaviorType">An open generic behavior type</param>
    /// <returns>This</returns>
    public void AddOpenBehavior(Type openBehaviorType)
    {
        if (!openBehaviorType.IsGenericType)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be generic");
        }

        var allPipelines = Array.FindAll(openBehaviorType.GetInterfaces(), OpenGenericPipelineBehaviorFilter);
            
        if (allPipelines.Length is 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IPipelineBehavior<>).FullName} or {typeof(IPipelineBehavior<,>).FullName} or {typeof(IStreamPipelineBehavior<,>).FullName}");
        }

        BehaviorsToRegister.Add(openBehaviorType, allPipelines);
    }

    private static bool OpenGenericPipelineBehaviorFilter(Type interfaceImp)
    {
        if (!interfaceImp.ContainsGenericParameters && interfaceImp.GetGenericArguments().Length is not 2)
        {
            return false;
        }

        return interfaceImp == typeof(IPipelineBehavior<>) ||
               interfaceImp == typeof(IPipelineBehavior<,>) ||
               interfaceImp == typeof(IStreamPipelineBehavior<,>);
    }
}