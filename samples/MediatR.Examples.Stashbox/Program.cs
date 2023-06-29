using Stashbox;
using System;
using System.IO;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.MicrosoftDICExtensions;
using Microsoft.Extensions.DependencyInjection;
using Stashbox.Lifetime;

namespace MediatR.Examples.Stashbox;

class Program
{
    static Task Main()
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);
        return Runner.Run(mediator, writer, "Stashbox", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var container = new StashboxContainer()
            .RegisterInstance<TextWriter>(writer)
            .ConfigureMediatR(config =>
            {
               config.RegisterServicesFromAssemblyContaining<Ping>(); 
            });

        return container.GetRequiredService<IMediator>();
    }
}

internal static class StashboxContainerExtension
{
    public static IStashboxContainer ConfigureMediatR(this IStashboxContainer containerInstance, Action<MediatRServiceConfiguration<IStashboxContainer>> configuration)
    {
        var dependencyInjectionResgistrarConfiguration = new DependencyInjectionRegistrarAdapter<IStashboxContainer>(
            containerInstance,
            (container, serviceType, implementationType) => container.RegisterSingleton(implementationType, serviceType),
            (container, serviceType, implementationType) => container.Register(implementationType, serviceType, options => options.WithLifetime(Lifetimes.Transient)),
            (container, serviceType, implementationType) => container.RegisterSingleton(implementationType, serviceType),
            (container, serviceType, implementationType) => container.Register(implementationType, serviceType, options => options.WithLifetime(Lifetimes.Transient)),
            (container, serviceType, implementationType) => container.Register(implementationType, serviceType, options => options.ReplaceExisting().WithLifetime(Lifetimes.Singleton)),
            (container, serviceType, implementationType) => container.Register(implementationType, serviceType, options => options.ReplaceExisting().WithLifetime(Lifetimes.Transient)),
            (container, serviceType, implementationType) => container.Register(implementationType, serviceType, options => options.ReplaceExisting().WithLifetime(Lifetimes.Transient)),
            (container, serviceType, implementationType) => container.Register(implementationType, serviceType, options => options.ReplaceExisting().WithLifetime(Lifetimes.Singleton)),
            (container, fromType, toType) => container.Register(fromType, toType, options => options.WithLifetime(Lifetimes.Singleton)),
            (container, serviceType, instance) => container.RegisterInstance(instance, serviceType));

        MediatRConfigurator.ConfigureMediatR(dependencyInjectionResgistrarConfiguration, configuration);

        return containerInstance;
    }
}