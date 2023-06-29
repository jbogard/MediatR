using Autofac.Extensions.DependencyInjection;
using MediatR.Abstraction;
using MediatR.MicrosoftDICExtensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using MediatR.DependencyInjection;

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
    public static ContainerBuilder ConfigureMediatR(this ContainerBuilder builder, Action<MediatRServiceConfiguration<ContainerBuilder>> configuration)
    {
        var dependencyResolver = new DependencyInjectionRegistrarAdapter<ContainerBuilder>(
            builder,
            (containerBuilder, serviceType, implementingType) => containerBuilder.RegisterType(implementingType).As(serviceType).SingleInstance().PreserveExistingDefaults(),
            (containerBuilder, type) => containerBuilder.RegisterType(type).AsSelf().SingleInstance().PreserveExistingDefaults(),
            (containerBuilder, type) => containerBuilder.RegisterGeneric(type).AsSelf().SingleInstance().IfNotRegistered(type),
            (containerBuilder, type, instance) => containerBuilder.RegisterInstance(instance).As(type).SingleInstance().IfNotRegistered(type),
            (containerBuilder, fromType, toType) => containerBuilder.Register(c => c.Resolve(fromType)).As(toType).SingleInstance(),
            (containerBuilder, fromType, toType) => containerBuilder.Register(c => c.Resolve(fromType)).As(toType).SingleInstance().PreserveExistingDefaults(),
            (containerBuilder, fromType, toType) => containerBuilder.RegisterGeneric((c, genericTypes) => c.Resolve(fromType.MakeGenericType(genericTypes))).As(toType).SingleInstance(),
            (containerBuilder, fromType, toTypes) => containerBuilder.RegisterGeneric((c, genericTypeParameter) => c.Resolve(fromType.MakeGenericType(genericTypeParameter))).As(toTypes).SingleInstance(),
            (containerBuilder, serviceType, implementingType) => containerBuilder.RegisterType(implementingType).As(serviceType).InstancePerRequest(),
            (containerBuilder, serviceType, implementingType) => containerBuilder.RegisterGeneric(implementingType).As(serviceType).InstancePerRequest(),
            (containerBuilder, serviceType, implementingType) => containerBuilder.RegisterType(implementingType).As(serviceType).InstancePerRequest().PreserveExistingDefaults(),
            (containerBuilder, serviceType, implementingType) => containerBuilder.RegisterGeneric(implementingType).As(serviceType).InstancePerRequest().IfNotRegistered(serviceType));

        var config = new MediatRServiceConfiguration<ContainerBuilder>(dependencyResolver);

        configuration(config);
        
        MediatRConfigurator.ConfigureMediatR(config);
        
        return builder;
    }
}