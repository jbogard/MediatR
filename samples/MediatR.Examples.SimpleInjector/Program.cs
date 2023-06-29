using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.MicrosoftDICExtensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using MediatR.DependencyInjection;
using SimpleInjector;

namespace MediatR.Examples.SimpleInjector;

internal static class Program
{
    private static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "SimpleInjector", true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var container = new Container();
        var services = new ServiceCollection();

        services.AddSimpleInjector(container);

        container.ConfigureMediarR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<Ping>();
        })
            .RegisterInstance(writer.GetType(), writer);

        services.BuildServiceProvider().UseSimpleInjector(container);

        container.RegisterInstance<IServiceProvider>(container);

        var mediator = container.GetRequiredService<IMediator>();

        return mediator;
    }
}

internal static class ContainerExtension
{
    public static Container ConfigureMediarR(this Container containerInstance, Action<MediatRServiceConfiguration<Container>> configuration)
    {
        var dependencyRegistrarationConfiguration = new DependencyInjectionRegistrarAdapter<Container>(
            containerInstance,
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Singleton),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Singleton),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Singleton),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Singleton),
            (container, fromType, toType) => container.Register(toType, fromType, Lifestyle.Singleton),
            (container, serviceType, instance) => container.RegisterInstance(serviceType, instance));

        MediatRConfigurator.ConfigureMediatR(dependencyRegistrarationConfiguration, configuration);

        return containerInstance;
    }
}