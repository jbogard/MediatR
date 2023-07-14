using MediatR.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MicrosoftDICExtensions;

public sealed class MediatRMicrosoftDicConfiguration : MediatRServiceConfiguration<IServiceCollection>
{
    /// <summary>
    /// Gets or sets the mapping registration live time. If not set is defaults to <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime MappingServiceLifetime { get; set; } = ServiceLifetime.Singleton;

    public MediatRMicrosoftDicConfiguration(DependencyInjectionRegistrarAdapter<IServiceCollection> adapter)
        : base(adapter)
    {
    }
}