using System;
using System.IO;
using System.Threading.Tasks;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.LightInject;

class Program
{
    static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "LightInject");
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var serviceContainer = new ServiceContainer(ContainerOptions.Default.WithMicrosoftSettings());
        serviceContainer
            .ConfigureMediatR(config =>
                {
                    config.RegisterServicesFromAssemblyContaining<Ping>();
                })
            .RegisterInstance<TextWriter>(writer);

        var services = new ServiceCollection();
        var provider = serviceContainer.CreateServiceProvider(services);
        serviceContainer.Compile();
        return provider.GetRequiredService<IMediator>(); 
    }
}

internal static class ServiceContainerExtension
{
    public static ServiceContainer ConfigureMediatR(this ServiceContainer serviceContainer, Action<LightInjectConfiguration> configuration)
    {
        var config = new LightInjectConfiguration();

        configuration(config);

        var adapter = new LightInjectContainerAdapter(serviceContainer, config);
        
        MediatRConfigurator.Configure(adapter, config);

        return serviceContainer;
    }

    public sealed class LightInjectConfiguration : MediatRServiceConfiguration
    {
        public ILifetime GetMappingLifetime() => new PerContainerLifetime();
    }

    internal sealed class LightInjectContainerAdapter : DependencyInjectionRegistrarAdapter<ServiceContainer, LightInjectConfiguration>
    {
        public LightInjectContainerAdapter(ServiceContainer registrar, LightInjectConfiguration configuration)
            : base(registrar, configuration)
        {
        }

        public override void RegisterInstance(Type serviceType, object instance) =>
            Registrar.RegisterInstance(serviceType, instance);

        public override void RegisterSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterSingleton(serviceType, implementationType);

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterSingleton(serviceType, implementationType);

        public override void RegisterMapping(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType, Configuration.GetMappingLifetime());

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType, Configuration.GetMappingLifetime());

        public override void Register(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType);

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType);

        public override bool IsAlreadyRegistered(Type serviceType, Type implementationType)
        {
            foreach (var serviceRegistration in Registrar.AvailableServices)
            {
                if (serviceRegistration.ServiceType == serviceType && serviceRegistration.ImplementingType == implementationType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}