using Stashbox;
using System;
using System.IO;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                config.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
                config.RegisterServicesFromAssemblyContaining<Ping>(); 
            });

        return container.GetRequiredService<IMediator>();
    }
}

internal static class StashboxContainerExtension
{
    public static IStashboxContainer ConfigureMediatR(this IStashboxContainer containerInstance, Action<StashBoxConfiguration> configuration)
    {
        var config = new StashBoxConfiguration();

        configuration(config);

        var adapter = new StashBoxAdapter(containerInstance, config);

        MediatRConfigurator.Configure(adapter, config);

        return containerInstance;
    }

    public sealed class StashBoxConfiguration : MediatRServiceConfiguration
    {
    }

    internal sealed class StashBoxAdapter : DependencyInjectionRegistrarAdapter<IStashboxContainer, StashBoxConfiguration>
    {
        public StashBoxAdapter(IStashboxContainer registrar, StashBoxConfiguration configuration)
            : base(registrar, configuration)
        {
        }

        public override void RegisterInstance(Type serviceType, object instance) =>
            Registrar.Register(serviceType, options => options.WithInstance(instance));

        public override void RegisterSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterSingleton(serviceType, implementationType);

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterSingleton(serviceType, implementationType);

        public override void RegisterMapping(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, option => option.WithFactory(c => c.GetRequiredService(implementationType)));

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, option => option.WithFactory(c => c.GetRequiredService(implementationType)));

        public override void Register(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType);

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType);

        public override bool IsAlreadyRegistered(Type serviceType, Type implementationType) =>
            Registrar.IsRegistered(implementationType);
    }
}