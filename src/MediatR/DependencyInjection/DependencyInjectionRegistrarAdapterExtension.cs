using MediatR.DependencyInjection.ConfigurationBase;
using System;

namespace MediatR.DependencyInjection;

internal static class DependencyInjectionRegistrarAdapterExtension
{
    public static void Register<TRegistrar, TConfiguration>(
        this DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter,
        MediatRServiceConfiguration configuration,
        Type implementationType,
        params (Type Interface, bool MustBeSingleRegistration)[] serviceTypes)
        where TConfiguration : MediatRServiceConfiguration
    {
        if (configuration.RegistrationStyle == RegistrationStyle.EachServiceOneInstance)
        {
            foreach (var (serviceType, mustBeSingleRegistration) in serviceTypes)
            {
                if (mustBeSingleRegistration)
                {
                    adapter.RegisterOnlyOnce(serviceType, implementationType);
                }
                else
                {
                    adapter.Register(serviceType, implementationType);
                }
            }
        }
        else
        {
            adapter.RegisterSelfSingleton(implementationType);
            foreach (var (serviceType, mustBeSingleRegistration) in serviceTypes)
            {
                if (mustBeSingleRegistration)
                {
                    adapter.RegisterMappingOnlyOnce(serviceType, implementationType);
                }
                else
                {
                    adapter.RegisterMapping(serviceType, implementationType);
                }
            }
        }
    }

    public static void RegisterOpenGeneric<TRegistrar, TConfiguration>(
        this DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter,
        MediatRServiceConfiguration configuration,
        Type implementationType,
        params (Type Interface, bool MustBeSingleRegistration)[] serviceTypes)
        where TConfiguration : MediatRServiceConfiguration
    {
        if (configuration.RegistrationStyle == RegistrationStyle.EachServiceOneInstance)
        {
            foreach (var (serviceType, mustBeSingleRegistration) in serviceTypes)
            {
                if (mustBeSingleRegistration)
                {
                    adapter.RegisterOpenGenericOnlyOnce(serviceType, implementationType);
                }
                else
                {
                    adapter.RegisterOpenGeneric(serviceType, implementationType);
                }
            }
        }
        else
        {
            adapter.RegisterOpenGenericSelfSingleton(implementationType);
            foreach (var (serviceType, mustBeSingleRegistration) in serviceTypes)
            {
                if (mustBeSingleRegistration)
                {
                    adapter.RegisterOpenGenericMappingOnlyOnce(serviceType, implementationType);
                }
                else
                {
                    adapter.RegisterOpenGenericMapping(serviceType, implementationType);
                }
            }
        }
    }
}