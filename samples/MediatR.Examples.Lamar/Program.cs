using System;
using System.IO;
using System.Threading.Tasks;
using Lamar;
using Lamar.IoC.Instances;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.MicrosoftDICExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    public static ServiceRegistry ConfigureMediatR(this ServiceRegistry serviceRegistry, Action<MediatRServiceConfiguration<ServiceRegistry>> configuration)
    {
        var dependencyRegistrarConfiguration = new DependencyInjectionRegistrarAdapter<ServiceRegistry>(
            serviceRegistry,
            (registry, serviceType, implementationType) => registry.For(serviceType).Use(implementationType).Singleton(),
            (registry, serviceType, implementationType) => registry.For(serviceType).Use(implementationType).Transient(),
            (registry, serviceType, implementationType) => registry.For(serviceType).Use(implementationType).Singleton(),
            (registry, serviceType, implementationType) => registry.For(serviceType).Use(implementationType).Singleton(),
            (registry, serviceType, implementationType) => registry.TryAddEnumerable(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton)),
            (registry, serviceType, implementationType) => registry.TryAddEnumerable(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient)),
            (registry, serviceType, implementationType) => registry.TryAddEnumerable(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient)),
            (registry, serviceType, implementationType) => registry.TryAddEnumerable(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton)),
            (registry, fromType, toType) => registry.For(toType).Use(fromType),
            (registry, serviceType, instance) => registry.For(serviceType).Use(new ObjectInstance(serviceType, instance)));

        MediatRConfigurator.ConfigureMediatR(dependencyRegistrarConfiguration, configuration);

        return serviceRegistry;
    }
}