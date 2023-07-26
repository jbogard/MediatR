using Autofac.Extensions.DependencyInjection;
using MediatR.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.ConfigurationBase;

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

        public override void RegisterInstance(Type serviceType, object instance) => throw new NotImplementedException();

        public override void RegisterSingleton(Type serviceType, Type implementationType) => throw new NotImplementedException();

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) => throw new NotImplementedException();

        public override void RegisterMapping(Type serviceType, Type implementationType) => throw new NotImplementedException();

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType) => throw new NotImplementedException();

        public override void Register(Type serviceType, Type implementationType) => throw new NotImplementedException();

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType) => throw new NotImplementedException();

        public override bool IsAlreadyRegistered(Type serviceType, Type implementationType) => throw new NotImplementedException();
    }
}