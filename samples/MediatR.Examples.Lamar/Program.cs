using System;
using System.IO;
using System.Threading.Tasks;
using Lamar;
using Lamar.IoC.Instances;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.ConfigurationBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.Examples.Lamar;

class Program
{
    static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "Lamar");
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var container = new Container(cfg =>
        {
            cfg.ConfigureMediatR(config =>
            {
                config.RegisterServicesFromAssemblyContaining<Ping>();
            });

            cfg.For<TextWriter>().Use(writer);
        });

        var mediator = container.GetInstance<IMediator>();

        return mediator;
    }
}

internal static class ServiceRegistrarExtension
{
    public static ServiceRegistry ConfigureMediatR(this ServiceRegistry serviceRegistry, Action<LamarConfiguration> configuration)
    {
        var config = new LamarConfiguration();

        configuration(config);

        var adapter = new LamarServiceRegistryAdapter(serviceRegistry, config);

        MediatRConfigurator.Configure(adapter, config);

        return serviceRegistry;
    }

    public sealed class LamarConfiguration : MediatRServiceConfiguration
    {
    }

    internal sealed class LamarServiceRegistryAdapter : DependencyInjectionRegistrarAdapter<ServiceRegistry, LamarConfiguration>
    {
        public LamarServiceRegistryAdapter(ServiceRegistry registrar, LamarConfiguration configuration)
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