using System;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MicrosoftDiCExtensions;

internal sealed class MappedServiceDescriptor : ServiceDescriptor
{
    public new Type ImplementationType { get; }

    public MappedServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        : base(serviceType, implementationType, lifetime) =>
        ImplementationType = implementationType;

    public MappedServiceDescriptor(Type serviceType, object instance)
        : base(serviceType, instance)=>
        throw new NotSupportedException();

    public MappedServiceDescriptor(Type serviceType, Type implementationType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        : base(serviceType, factory, lifetime)
    {
        ImplementationType = implementationType;
    }
}