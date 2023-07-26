using System;
using MediatR.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MicrosoftDiCExtensions;

internal sealed class ServiceCollectionAdapter : DependencyInjectionRegistrarAdapter<IServiceCollection, ServiceCollectionConfiguration>
{
    public ServiceCollectionAdapter(IServiceCollection registrar, ServiceCollectionConfiguration configuration)
        : base(registrar, configuration)
    {
    }

    public override void RegisterInstance(Type serviceType, object instance) =>
        Registrar.AddSingleton(serviceType, instance);

    public override void RegisterSingleton(Type serviceType, Type implementationType) =>
        Registrar.AddSingleton(serviceType, implementationType);

    public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType)
    {
        var genericServiceType = serviceType.GetGenericTypeDefinition();
        HasSameArityCheck(genericServiceType, implementationType);
        Registrar.AddSingleton(genericServiceType, implementationType);
    }

    public override void RegisterMapping(Type serviceType, Type implementationType) =>
        Registrar.Add(new MappedServiceDescriptor(serviceType, implementationType, sp => sp.GetService(implementationType), Configuration.MappingLifetime));

    public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType)
    {
        var genericServiceType = serviceType.GetGenericTypeDefinition();
        HasSameArityCheck(genericServiceType, implementationType);

        if (!CheckIfOpenGenericMappingIsSupported(genericServiceType, implementationType))
        {
            return;
        }

        Registrar.Add(new MappedServiceDescriptor(genericServiceType, implementationType, sp => sp.GetService(implementationType), Configuration.MappingLifetime));
    }

    public override void Register(Type serviceType, Type implementationType) =>
        Registrar.Add(new ServiceDescriptor(serviceType, implementationType, Configuration.DefaultServiceLifetime));

    public override void RegisterOpenGeneric(Type serviceType, Type implementationType)
    {
        var genericServiceType = serviceType.GetGenericTypeDefinition();
        HasSameArityCheck(genericServiceType, implementationType);
        Registrar.Add(new ServiceDescriptor(genericServiceType, implementationType, Configuration.DefaultServiceLifetime));
    }

    public override bool IsAlreadyRegistered(Type serviceType, Type implementationType)
    {
        for (var i = 0; i < Registrar.Count; i++)
        {
            var descriptor = Registrar[i];
            if (descriptor.ServiceType == serviceType && IsImplementationType(descriptor, implementationType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsImplementationType(ServiceDescriptor serviceDescriptor, Type implementationType)
    {
        if (serviceDescriptor.ImplementationType is not null &&
            serviceDescriptor.ImplementationType == implementationType)
        {
            return true;
        }

        if (serviceDescriptor.ImplementationInstance is not null &&
            serviceDescriptor.ImplementationInstance.GetType() == implementationType)
        {
            return true;
        }

        return false;
    }

    private static void HasSameArityCheck(Type serviceType, Type implementationType)
    {
        if (serviceType.GetGenericArguments().Length != implementationType.GetGenericArguments().Length)
        {
            throw new InvalidOperationException("The implementation type and the service type must have the same generic arguments length as the implementation type.");
        }
    }

    private bool CheckIfOpenGenericMappingIsSupported(Type serviceType, Type implementationType)
    {
        // Service Collection only supports ONE open generic singleton instance with one open generic service.
        // Because the mapping is only used for the OneInstanceForeachService checking the last registration is sufficient to figure out if we can register the service.
        // If this method is used somewhere else this implementation needs to be check and eventually adapted.

        var lastRegisteredService = Registrar[Registrar.Count - 1];

        // Check if the last registration was a mapping registration with the same implementation type but different service type.
        // If true then we have multiple services for a single open generic instance.
        if (lastRegisteredService is MappedServiceDescriptor mappingServiceDescriptor &&
            mappingServiceDescriptor.ImplementationType == implementationType &&
            mappingServiceDescriptor.ServiceType != serviceType)
        {
            throw new NotSupportedException("Service Collection does not support mapping a single open generic instance to more then one open generic service types." +
                                            $" Can not register Implementation Type '{implementationType}' for Service Type '{serviceType}'." +
                                            $" Consider implementing '{serviceType}' in a new class to resolve this exception.");
        }

        // The last registration was the self singleton registration for this service type.
        // One is still supported. Therefor we need to remove the self singleton registration because it is not needed.
        // For the registration we register the potentially only service type for the implementation type.
        if (lastRegisteredService.ServiceType == implementationType &&
            lastRegisteredService.Lifetime == ServiceLifetime.Singleton)
        {
            Registrar.Remove(lastRegisteredService);
            Registrar.Add(new MappedServiceDescriptor(serviceType, implementationType, Configuration.MappingLifetime));
            return false;
        }

        // The last registration has to do nothing with this mapping. We can proceed as always.
        return true;
    }
}