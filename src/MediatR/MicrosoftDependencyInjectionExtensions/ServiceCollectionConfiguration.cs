using System;
using MediatR.Abstraction;
using MediatR.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MicrosoftDependencyInjectionExtensions;

/// <summary>
/// Defines the configuration for the ServiceCollection Dependency Injection Container.
/// </summary>
public sealed class ServiceCollectionConfiguration : MediatRServiceConfiguration
{
    /// <summary>
    /// Gets or sets the value for the mapping life times. Defaults to <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime MappingLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Gets or sets the value for the service lifetime. This also includes the <see cref="IMediator"/> it self.
    /// </summary>
    /// <remarks>
    /// This service life time will be applied on all service when <see cref="MediatRServiceConfiguration.RegistrationStyle"/> is set to <see cref="RegistrationStyle.EachServiceOneInstance"/>.
    /// </remarks>
    public ServiceLifetime DefaultServiceLifetime { get; set; } = ServiceLifetime.Transient;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();
        
        if (EnableCachingOfHandlers && (RegistrationStyle == RegistrationStyle.EachServiceOneInstance || DefaultServiceLifetime == ServiceLifetime.Singleton))
        {
            throw new InvalidOperationException($"Caching is only possible if the handlers are registered as singletons. Currently they are '{RegistrationStyle}' and that could break the application. Either set the option to '{RegistrationStyle.OneInstanceForeachService}' or disable the caching of the handlers.");
        }
    }
}