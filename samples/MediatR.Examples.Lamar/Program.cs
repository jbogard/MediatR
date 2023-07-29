using System;
using System.IO;
using System.Threading.Tasks;
using Lamar;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.Lamar;

class Program
{
    static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "Lamar");
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var container = new Container(cfg =>
        {
            cfg.ConfigureMediatR(config =>
            {
                config.RegisterServicesFromAssemblyContaining<Ping>();
            });

            cfg.For<TextWriter>().Use(writer);
        });

        var mediator = container.GetInstance<IMediator>();

        return mediator;
    }
}

internal static class ServiceRegistrarExtension
{
    public static ServiceRegistry ConfigureMediatR(this ServiceRegistry serviceRegistry, Action<LamarConfiguration> configuration)
    {
        var config = new LamarConfiguration();

        configuration(config);

        var adapter = new LamarServiceRegistryAdapter(serviceRegistry, config);

        MediatRConfigurator.Configure(adapter, config);

        return serviceRegistry;
    }

    public sealed class LamarConfiguration : MediatRServiceConfiguration
    {
        public ServiceLifetime MappingLifetime { get; set; } = ServiceLifetime.Singleton;
    }

    internal sealed class LamarServiceRegistryAdapter : DependencyInjectionRegistrarAdapter<ServiceRegistry, LamarConfiguration>
    {
        public LamarServiceRegistryAdapter(ServiceRegistry registrar, LamarConfiguration configuration)
            : base(registrar, configuration)
        {
        }

        public override void RegisterInstance(Type serviceType, object instance) =>
            Registrar.AddSingleton(serviceType, instance);

        public override void RegisterSingleton(Type serviceType, Type implementationType) =>
            Registrar.AddSingleton(serviceType, implementationType);

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) =>
            Registrar.AddSingleton(serviceType, implementationType);

        public override void RegisterMapping(Type serviceType, Type implementationType) =>
            Registrar.Add(new ServiceDescriptor(serviceType, sp => sp.GetService(implementationType)!, Configuration.MappingLifetime));

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType) =>
            Registrar.Add(new ServiceDescriptor(serviceType, sp => sp.GetService(implementationType)!, Configuration.MappingLifetime));

        public override void Register(Type serviceType, Type implementationType) =>
            Registrar.AddSingleton(serviceType, implementationType);

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType) =>
            Registrar.AddSingleton(serviceType, implementationType);

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
    }
}