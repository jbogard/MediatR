using MediatR.DependencyInjection;
using MediatR.DependencyInjection.AssemblyScanner;
using MediatR.DependencyInjection.ConfigurationBase;
using MediatR.ExceptionHandling;
using MediatR.Subscriptions;

namespace MediatR;

/// <summary>
/// Provides Methods to configure MediatR.
/// </summary>
public static class MediatRConfigurator
{
    /// <summary>
    /// Configures MediatR library with its configuration.
    /// </summary>
    /// <param name="adapter">The dependency injection registration adapter.</param>
    /// <param name="configuration">The MediatR Configuration.</param>
    public static void Configure<TRegistrar, TConfiguration>(
        DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter,
        TConfiguration configuration)
        where TConfiguration : MediatRServiceConfiguration
    {
        configuration.Validate();
        
        SubscriptionFactory.Initialize(configuration);
        ExceptionHandlerFactory.Initialize(configuration);

        var scanner = new AssemblyScanner(configuration);
        scanner.ScanForMediatRServices(adapter);
    }
}