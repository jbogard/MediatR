using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.ConfigurationBase;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.LightInject;

class Program
{
    static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "LightInject");
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var serviceContainer = new ServiceContainer(ContainerOptions.Default.WithMicrosoftSettings());
        serviceContainer
            .ConfigureMediatR(config =>
                {
                    config.RegisterServicesFromAssemblyContaining<Ping>();
                })
            .RegisterInstance<TextWriter>(writer);

        var services = new ServiceCollection();
        var provider = serviceContainer.CreateServiceProvider(services);
        serviceContainer.Compile();
        return provider.GetRequiredService<IMediator>(); 
    }
}

internal static class ServiceContainerExtension
{
    public static ServiceContainer ConfigureMediatR(this ServiceContainer serviceContainer, Action<LightInjectConfiguration> configuration)
    {
        var config = new LightInjectConfiguration();

        configuration(config);

        var adapter = new LightInjectContainerAdapter(serviceContainer, config);
        
        MediatRConfigurator.Configure(adapter, config);

        return serviceContainer;
    }

    public sealed class LightInjectConfiguration : MediatRServiceConfiguration
    {
    }

    internal sealed class LightInjectContainerAdapter : DependencyInjectionRegistrarAdapter<ServiceContainer, LightInjectConfiguration>
    {
        public LightInjectContainerAdapter(ServiceContainer registrar, LightInjectConfiguration configuration)
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