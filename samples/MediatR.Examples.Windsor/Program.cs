using System.Threading.Tasks;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using System;
using System.Collections.Generic;
using System.IO;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;

namespace MediatR.Examples.Windsor;

internal class Program
{
    private static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);

        return Runner.Run(mediator, writer, "Castle.Windsor", true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var container = new WindsorContainer();

        container.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Ping>();
        });

        container.Register(Component.For<TextWriter>().Instance(writer));

        // *** The default lifestyle for Windsor is Singleton
        // *** If you are using ASP.net, it's better to register your services with 'Per Web Request LifeStyle'.

        var mediator = container.Resolve<IMediator>();

        return mediator;
    }
}

public static class WindsorExtension
{
    public static WindsorContainer AddMediatR(this WindsorContainer container, Action<WindsorConfiguration> configuration)
    {
        var config = new WindsorConfiguration();

        configuration(config);

        var adapter = new WindsorContainerAdapter(container, config);
        
        MediatRConfigurator.Configure(adapter, config);
        
        adapter.RegisterSingleton(typeof(IServiceProvider), typeof(ServiceProviderAdapter));
        adapter.RegisterInstance(typeof(IWindsorContainer), container);

        return container;
    }
    
    private sealed class ServiceProviderAdapter : IServiceProvider
    {
        private readonly IWindsorContainer _container;
        
        public ServiceProviderAdapter(IWindsorContainer container) => _container = container;

        public object? GetService(Type serviceType)
        {
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return _container.Kernel.ResolveAll(serviceType.GetGenericArguments()[0]);
            }

            return _container.Resolve(serviceType);
        }
    }
    
    public sealed class WindsorConfiguration : MediatRServiceConfiguration
    {
    }

    private sealed class WindsorContainerAdapter : DependencyInjectionRegistrarAdapter<WindsorContainer, WindsorConfiguration>
    {
        public WindsorContainerAdapter(WindsorContainer registrar, WindsorConfiguration configuration)
            : base(registrar, configuration) =>
            registrar.Kernel.Resolver.AddSubResolver(new EnumerableResolver(registrar.Kernel, true));

        public override void RegisterInstance(Type serviceType, object instance) =>
            Registrar.Register(Component.For(serviceType).Instance(instance));

        public override void RegisterSingleton(Type serviceType, Type implementationType) =>
            Registrar.Register(Component.For(serviceType).ImplementedBy(implementationType).LifestyleSingleton());

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) =>
            Registrar.Register(Component.For(serviceType).ImplementedBy(implementationType).LifestyleSingleton());

        public override void RegisterMapping(Type serviceType, Type implementationType) =>
            Registrar.Register(Component.For(serviceType).UsingFactoryMethod(kernel => kernel.Resolve(implementationType)).LifestyleSingleton());

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType) =>
            Registrar.Register(Component.For(serviceType).UsingFactoryMethod(kernel => kernel.Resolve(implementationType)).LifestyleSingleton());

        public override void Register(Type serviceType, Type implementationType) =>
            Registrar.Register(Component.For(serviceType).ImplementedBy(implementationType).Named($"{serviceType}_{implementationType}"));

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType) =>
            Registrar.Register(Component.For(serviceType).ImplementedBy(implementationType));

        public override bool IsAlreadyRegistered(Type serviceType, Type implementationType)
        {
            foreach (var handler in Registrar.Kernel.GetHandlers(serviceType))
            {
                if (handler.ComponentModel.Implementation == implementationType)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    private sealed class EnumerableResolver : CollectionResolver
    {
        public EnumerableResolver(IKernel kernel, bool allowEmptyCollections = false)
            : base(kernel, allowEmptyCollections)
        {
        }

        protected override Type GetItemType(Type targetItemType)
        {
            var arrayType = base.GetItemType(targetItemType);
            if (arrayType is not null)
            {
                return arrayType;
            }

            if (targetItemType.IsGenericType && targetItemType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return targetItemType.GetGenericArguments()[0];
            }

            return null!;
        }
    }
}