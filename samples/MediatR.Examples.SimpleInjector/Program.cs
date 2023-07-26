using System.Threading.Tasks;
using MediatR.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using MediatR.DependencyInjection;
using SimpleInjector;
using MediatR.DependencyInjection.ConfigurationBase;

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
    public static Container ConfigureMediarR(this Container containerInstance, Action<SimpleInjectorConfiguration> configuration)
    {
        var config = new SimpleInjectorConfiguration();

        configuration(config);

        var adapter = new SimpleInjectorAdapter(containerInstance, config);

        MediatRConfigurator.Configure(adapter, config);

        return containerInstance;
    }
    
    public sealed class SimpleInjectorConfiguration : MediatRServiceConfiguration
    {
    }
    
    internal sealed class SimpleInjectorAdapter : DependencyInjectionRegistrarAdapter<Container, SimpleInjectorConfiguration>
    {
        public SimpleInjectorAdapter(Container registrar, SimpleInjectorConfiguration configuration)
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