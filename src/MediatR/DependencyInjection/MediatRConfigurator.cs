using System;
using MediatR.Abstraction;
using MediatR.Abstraction.Pipeline;

namespace MediatR.DependencyInjection;

/// <summary>
///  Extensions to scan for MediatR handlers and registers them.
/// - Scans for any handler interface implementations and registers them.
/// - Scans for any <see cref="IRequestPreProcessor{TRequest}"/> and <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations and registers them.
/// Registers <see cref="IMediator"/> as a singleton instance.
/// After calling AddMediatR you can use the container to resolve a single <see cref="IMediator"/> instance.
/// If you have any 
/// </summary>
public static class MediatRConfigurator
{
    /// <summary>
    /// Registers MediatR for the registrar <typeparamref name="TRegistrar"/>.
    /// </summary>
    /// <param name="configuration">The MediatR Configuration.</param>
    /// <typeparam name="TRegistrar">The DiC Registrar Type</typeparam>
    /// <returns>The same registrar object.</returns>
    /// <exception cref="ArgumentException">When there are no assemblies to scan.</exception>
    public static TRegistrar ConfigureMediatR<TRegistrar>(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        if (configuration.AssembliesToRegister.Count is 0)
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
        }

        var scanner = new AssemblyScanner<TRegistrar>(configuration);
        scanner.RegisterMediatRServices();
        
        InternalServiceRegistrar.AddInternalServiceTypes(configuration);

        return configuration.DependencyInjectionRegistrarAdapter.Registrar;
    }
}