using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions to scan for MediatR handlers and registers them.
/// - Scans for any handler interface implementations and registers them as <see cref="ServiceLifetime.Transient"/>
/// - Scans for any <see cref="IRequestPreProcessor{TRequest}"/> and <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations and registers them as transient instances
/// Registers <see cref="IMediator"/> as a transient instance
/// After calling AddMediatR you can use the container to resolve an <see cref="IMediator"/> instance.
/// This does not scan for any <see cref="IPipelineBehavior{TRequest,TResponse}"/> instances including <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/> and <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/>.
/// To register behaviors, use the <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection,Type,Type)"/> with the open generic or closed generic types.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>        
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddMediatR(assemblies, configuration: null);

    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration>? configuration, params Assembly[] assemblies)
        => services.AddMediatR(assemblies, configuration);

    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Assembly> assemblies, Action<MediatRServiceConfiguration>? configuration)
    {
        if (!assemblies.Any())
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
        }
        var serviceConfig = new MediatRServiceConfiguration();

        configuration?.Invoke(serviceConfig);

        ServiceRegistrar.AddRequiredServices(services, serviceConfig);

        ServiceRegistrar.AddMediatRClasses(services, assemblies, serviceConfig);

        return services;
    }

    /// <summary>
    /// Registers handlers and mediator types from the assemblies that contain the specified types
    /// </summary>
    /// <param name="services"></param>
    /// <param name="handlerAssemblyMarkerTypes"></param>        
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
        => services.AddMediatR(handlerAssemblyMarkerTypes, configuration: null);
        
    /// <summary>
    /// Registers handlers and mediator types from the assemblies that contain the specified types
    /// </summary>
    /// <param name="services"></param>
    /// <param name="handlerAssemblyMarkerTypes"></param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration>? configuration, params Type[] handlerAssemblyMarkerTypes)
        => services.AddMediatR(handlerAssemblyMarkerTypes, configuration);

    /// <summary>
    /// Registers handlers and mediator types from the assemblies that contain the specified types
    /// </summary>
    /// <param name="services"></param>
    /// <param name="handlerAssemblyMarkerTypes"></param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Type> handlerAssemblyMarkerTypes, Action<MediatRServiceConfiguration>? configuration)
        => services.AddMediatR(handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly), configuration);
}