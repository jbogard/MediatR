using System;
using System.IO;
using System.Threading.Tasks;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
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
    public static IContainer ConfigureMediatR(this IContainer containerInstance, Action<DryIoCContainerConfiguration> configuration)
    {
        var config = new DryIoCContainerConfiguration();
        configuration(config);

        var adapter = new DryIoCContainerAdapter(containerInstance, config);

        MediatRConfigurator.Configure(adapter, config);

        return containerInstance;
    }

    public sealed class DryIoCContainerConfiguration : MediatRServiceConfiguration
    {
        public IReuse MappingLifeTime { get; set; } = Reuse.Singleton;
    }

    internal sealed class DryIoCContainerAdapter : DependencyInjectionRegistrarAdapter<IContainer, DryIoCContainerConfiguration>
    {
        public DryIoCContainerAdapter(IContainer registrar, DryIoCContainerConfiguration configuration)
            : base(registrar, configuration)
        {
        }

        public override void RegisterInstance(Type serviceType, object instance) =>
            Registrar.RegisterInstance(serviceType, instance);

        public override void RegisterSingleton(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType, Reuse.Singleton);

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType, Reuse.Singleton);

        public override void RegisterMapping(Type serviceType, Type implementationType) =>
            Registrar.RegisterMapping(serviceType, implementationType, Configuration.MappingLifeTime);

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType) =>
            Registrar.RegisterMapping(serviceType, implementationType, Configuration.MappingLifeTime);

        public override void Register(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType);

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType) =>
            Registrar.Register(serviceType, implementationType);

        public override bool IsAlreadyRegistered(Type serviceType, Type implementationType) =>
            Registrar.IsRegistered(serviceType, condition: factory => factory.ImplementationType == implementationType);
    }
}