using MediatR.Abstraction;
using MediatR.DependencyInjection.ConfigurationBase;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MicrosoftDiCExtensions;

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
}