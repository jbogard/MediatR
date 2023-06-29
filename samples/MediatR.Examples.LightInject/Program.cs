using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using MediatR.Pipeline;
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
    public static ServiceContainer ConfigureMediatR(this ServiceContainer serviceContainer, Action<MediatRServiceConfiguration<ServiceContainer>> configuration)
    {
        var dependencyInjectionConfiguration = new DependencyInjectionRegistrarAdapter<ServiceContainer>(
            serviceContainer,
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, new PerContainerLifetime()),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, new PerRequestLifeTime()),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, new PerContainerLifetime()),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, new PerRequestLifeTime()),
            (container, serviceType, implementationType) =>
            {
                if (!container.ServiceExists(serviceType, implementationType))
                {
                    container.Register(serviceType, implementationType, new PerContainerLifetime());
                }
            },
            (container, serviceType, implementationType) =>
            {
                if (!container.ServiceExists(serviceType, implementationType))
                {
                    container.Register(serviceType, implementationType, new PerRequestLifeTime());
                }
            },
            (container, serviceType, implementationType) =>
            {
                if (!container.ServiceExists(serviceType, implementationType))
                {
                    container.Register(serviceType, implementationType, new PerRequestLifeTime());
                }
            },
            (container, serviceType, implementationType) =>
            {
                if (!container.ServiceExists(serviceType, implementationType))
                {
                    container.Register(serviceType, implementationType, new PerContainerLifetime());
                }
            },
            (container, fromType, toType) => container.Register(toType, fromType, new PerContainerLifetime()),
            (container, serviceType, instance) => container.RegisterInstance(serviceType, instance));

        MediatRConfigurator.ConfigureMediatR(dependencyInjectionConfiguration, configuration);

        return serviceContainer;
    }

    private static bool ServiceExists(this IServiceRegistry container, Type serviceType, Type implementingType) =>
        container.AvailableServices.Any(serviceRegistration =>
            serviceRegistration.ServiceType == serviceType &&
            serviceRegistration.ImplementingType == implementingType);
}