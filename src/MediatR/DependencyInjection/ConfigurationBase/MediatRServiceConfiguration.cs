using System;
using System.Collections.Generic;
using System.Reflection;
using MediatR.Abstraction;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Processors;
using MediatR.NotificationPublishers;

namespace MediatR.DependencyInjection.ConfigurationBase;

/// <summary>
/// Defines the configuration for the base MediatR Library.
/// </summary>
public abstract class MediatRServiceConfiguration
{
    /// <summary>
    /// Optional filter for types to register. Default value is a function returning true.
    /// </summary>
    public Func<Type, bool> TypeEvaluator { get; set; } = _ => true;

    /// <summary>
    /// Strategy for publishing notifications. Defaults to <see cref="ForeachAwaitPublisher"/>
    /// </summary>
    public INotificationPublisher NotificationPublisher { get; set; } = new ForeachAwaitPublisher();

    /// <summary>
    /// Type of notification publisher strategy to register. If set, overrides <see cref="NotificationPublisher"/>
    /// </summary>
    public Type? NotificationPublisherType { get; set; }

    /// <summary>
    /// Registration pattern to register services. Default value is <see cref="RegistrationStyle.EachServiceOneInstance"/>
    /// </summary>
    public RegistrationStyle RegistrationStyle { get; set; } = RegistrationStyle.EachServiceOneInstance;

    /// <summary>
    /// Request exception action processor strategy. Default value is <see cref="RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions"/>
    /// </summary>
    public RequestExceptionActionProcessorStrategy RequestExceptionActionProcessorStrategy { get; set; }
        = RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions;

    /// <summary>
    /// List of behaviors to register in specific order
    /// </summary>
    public TypeRegistrar RequestBehaviors { get; } = new(typeof(IPipelineBehavior<>), false);

    /// <summary>
    /// List of behaviors to register in specific order
    /// </summary>
    public TypeRegistrar RequestResponseBehaviors { get; } = new(typeof(IPipelineBehavior<,>), false);

    /// <summary>
    /// List of stream behaviors to register in specific order
    /// </summary>
    public TypeRegistrar StreamRequestBehaviors { get; } = new(typeof(IStreamPipelineBehavior<,>), false);

    /// <summary>
    /// List of request pre processors to register in specific order
    /// </summary>
    public TypeRegistrar RequestPreProcessors { get; } = new(typeof(IRequestPreProcessor<>), false);

    /// <summary>
    /// List of request response pre processors to register in specific order
    /// </summary>
    public TypeRegistrar RequestResponsePreProcessors { get; } = new(typeof(IRequestPreProcessor<,>), false);

    /// <summary>
    /// List of request post processors to register in specific order
    /// </summary>
    public TypeRegistrar RequestPostProcessors { get; } = new(typeof(IRequestPostProcessor<>), false);

    /// <summary>
    /// List of request post processors to register in specific order
    /// </summary>
    public TypeRegistrar RequestResponsePostProcessors { get; } = new(typeof(IRequestPostProcessor<,>), false);

    /// <summary>
    /// Enables MediatR to cache every handler when it was called by the mediator.
    /// </summary>
    /// <remarks>
    /// This is only for performance purpose and required that all handlers are registered as singletons.
    /// </remarks>
    public bool EnableCachingOfHandlers { get; set; }

    internal HashSet<Assembly> AssembliesToRegister { get; } = new();

    /// <summary>
    /// Register various handlers from assembly containing given type.
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
    public MediatRServiceConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        AssembliesToRegister.UnionWith(assemblies);

        return this;
    }

    /// <summary>
    /// Validates the configuration for any errors.
    /// </summary>
    public virtual void Validate()
    {
        if (AssembliesToRegister.Count is 0)
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
        }

        if (EnableCachingOfHandlers && RegistrationStyle == RegistrationStyle.EachServiceOneInstance)
        {
            throw new InvalidOperationException($"Caching is only possible if the handlers are registered as singletons. Currently they are '{RegistrationStyle}' and that could break the application. Either set the option to '{RegistrationStyle.OneInstanceForeachService}' or disable the caching of the handlers.");
        }
    }
}