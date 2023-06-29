using System;
using System.IO;
using System.Threading.Tasks;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.DryIoc;

class Program
{
    static Task Main()
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "DryIoc");
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var container = new Container();
        container.ConfigureMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<Ping>();
        });
        container.Use<TextWriter>(writer);

        var services = new ServiceCollection();

        var adapterContainer = container.WithDependencyInjectionAdapter(services);

        return adapterContainer.GetRequiredService<IMediator>();
    }
}

internal static class DryIoCException
{
    public static IContainer ConfigureMediatR(this IContainer containerInstance, Action<MediatRServiceConfiguration<IContainer>> configuration)
    {
        var dependencyRegistrarAdapter = new DependencyInjectionRegistrarAdapter<IContainer>(
            containerInstance,
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Reuse.Singleton),
            (container, type) => container.Register(type, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep),
            (container, type) => container.Register(type, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep),
            (container, type, instance) => container.RegisterInstance(type, instance),
            (container, fromType, toType) => container.RegisterMapping(toType, fromType),
            (container, fromType, toType) => container.RegisterMapping(toType, fromType, IfAlreadyRegistered.Keep),
            (container, fromType, toTypes) =>
            {
                foreach (var toType in toTypes)
                {
                    container.RegisterMapping(toType, fromType);
                }
            },
            (container, fromType, toTypes) =>
            {
                foreach (var toType in toTypes)
                {
                    if (!container.IsRegistered(toType, condition: factory => factory.ImplementationType == fromType))
                    {
                        container.RegisterMapping(toType, fromType);
                    }
                }
            },
            (container, serviceType, implementingType) => container.Register(serviceType, implementingType, Reuse.Transient),
            (container, serviceType, implementingType) => container.Register(serviceType, implementingType, Reuse.Transient),
            (container, serviceType, implementingType) => container.Register(serviceType, implementingType, Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Keep),
            (container, serviceType, implementingType) => container.Register(serviceType, implementingType, Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Keep));

        var config = new MediatRServiceConfiguration<IContainer>(dependencyRegistrarAdapter);

        configuration(config);

        MediatRConfigurator.ConfigureMediatR(config);

        return containerInstance;
    }
}