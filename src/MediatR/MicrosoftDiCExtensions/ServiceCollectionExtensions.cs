using System;
using MediatR.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MicrosoftDiCExtensions;

/// <summary>
/// Extensions to scan for MediatR handlers and registers them.
/// - Scans for all MediatR interface implementations and registers them.
/// Registers <see cref="IMediator"/> as a  instance
/// After calling AddMediatR you can use the container to resolve an <see cref="IMediator"/> instance.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies.
    /// </summary>
    /// <param name="services">The Service collection.</param>
    /// <param name="configuration">The action used to configure the options.</param>
    /// <returns>The same Service collection.</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, Action<ServiceCollectionConfiguration> configuration)
    {
        var config = new ServiceCollectionConfiguration();

        configuration(config);

        return services.AddMediatR(config);
    }

    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies.
    /// </summary>
    /// <param name="services">The Service collection.</param>
    /// <param name="configuration">The Configuration options.</param>
    /// <returns>The same Service collection.</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, ServiceCollectionConfiguration configuration)
    {
        var adapter = new ServiceCollectionAdapter(services, configuration);

        MediatRConfigurator.Configure(adapter, configuration);

        return services;
    }
}