using Autofac.Extensions.DependencyInjection;
using MediatR.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;

namespace MediatR.Examples.Autofac;

internal static class Program
{
    public static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "Autofac", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var builder = new ContainerBuilder();

        builder.ConfigureMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<Ping>();
        })
            .RegisterInstance(writer).As<TextWriter>();

        var services = new ServiceCollection();

        builder.Populate(services);

        var container = builder.Build();
        var serviceProvider = new AutofacServiceProvider(container);

        return serviceProvider.GetRequiredService<IMediator>();
    }
}

internal static class AutoFactBuilderExtension
{
    public static ContainerBuilder ConfigureMediatR(this ContainerBuilder builder, Action<ContainerBuilderConfiguration> configuration)
    {
        var config = new ContainerBuilderConfiguration();
        
        configuration(config);

        var adapter = new ContainerBuilderAdapter(builder, config);
        
        MediatRConfigurator.Configure(adapter, config);
        
        return builder;
    }

    public sealed class ContainerBuilderConfiguration : MediatRServiceConfiguration
    {
    }

    internal sealed class ContainerBuilderAdapter : DependencyInjectionRegistrarAdapter<ContainerBuilder, ContainerBuilderConfiguration>
    {
        public ContainerBuilderAdapter(ContainerBuilder registrar, ContainerBuilderConfiguration configuration)
            : base(registrar, configuration)
        {
        }

        public override void RegisterInstance(Type serviceType, object instance) =>
            Registrar.RegisterInstance(instance).As(serviceType);

        public override void RegisterSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterType(implementationType).As(serviceType).SingleInstance();

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterGeneric(implementationType).As(serviceType);

        public override void RegisterMapping(Type serviceType, Type implementationType) =>
            Registrar.Register(context => context.Resolve(implementationType)).As(serviceType);

        public override void RegisterMappingOnlyOnce(Type serviceType, Type implementationType) =>
            Registrar.Register(context => context.Resolve(implementationType)).As(serviceType).PreserveExistingDefaults();

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType) =>
            Registrar.RegisterGeneric((context, types) => context.Resolve(implementationType.MakeGenericType(types))).As(serviceType);

        public override void RegisterOpenGenericMappingOnlyOnce(Type serviceType, Type implementationType) =>
            Registrar.RegisterGeneric((context, types) => context.Resolve(implementationType.MakeGenericType(types))).As(serviceType).IfNotRegistered(serviceType);

        public override void Register(Type serviceType, Type implementationType) =>
            Registrar.RegisterType(implementationType).As(serviceType).SingleInstance();

        public override void RegisterOnlyOnce(Type serviceType, Type implementationType) =>
            Registrar.RegisterType(implementationType).As(serviceType).PreserveExistingDefaults();

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType) =>
            Registrar.RegisterGeneric(implementationType).As(serviceType).SingleInstance();

        public override void RegisterOpenGenericOnlyOnce(Type serviceType, Type implementationType) =>
            Registrar.RegisterGeneric(implementationType).As(serviceType).IfNotRegistered(serviceType);

        public override bool IsAlreadyRegistered(Type serviceType, Type implementationType) =>
            throw new NotSupportedException();
    }
}