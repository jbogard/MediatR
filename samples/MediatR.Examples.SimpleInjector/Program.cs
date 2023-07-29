using System.Threading.Tasks;
using MediatR.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
using SimpleInjector;

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
            .RegisterInstance(typeof(TextWriter), writer);

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

        using var adapter = new SimpleInjectorAdapter(containerInstance, config);

        MediatRConfigurator.Configure(adapter, config);

        return containerInstance;
    }
    
    public sealed class SimpleInjectorConfiguration : MediatRServiceConfiguration
    {
    }
    
    internal sealed class SimpleInjectorAdapter : DependencyInjectionRegistrarAdapter<Container, SimpleInjectorConfiguration>, IDisposable
    {
        private readonly List<(Type ServiceType, Type ImplementationType)> _pipelines = new();
        private readonly List<(Type ServiceType, Type ImplementationType)> _notificationHandlers = new();

        public SimpleInjectorAdapter(Container registrar, SimpleInjectorConfiguration configuration)
            : base(registrar, configuration)
        {
        }

        public override void RegisterInstance(Type serviceType, object instance) =>
            Registrar.RegisterInstance(serviceType, instance);

        public override void RegisterSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterSingleton(serviceType, implementationType);

        public override void RegisterOpenGenericSingleton(Type serviceType, Type implementationType) =>
            Registrar.RegisterSingleton(serviceType.GetGenericTypeDefinition(), implementationType);

        public override void RegisterMapping(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType, () => Registrar.GetInstance(implementationType));
        }

        public override void RegisterMappingOnlyOnce(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType, () => Registrar.GetInstance(implementationType));
        }

        public override void RegisterOpenGenericMapping(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType.GetGenericTypeDefinition(), () => Registrar.GetInstance(implementationType));
        }

        public override void RegisterOpenGenericMappingOnlyOnce(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType.GetGenericTypeDefinition(), () => Registrar.GetInstance(implementationType));
        }

        public override void Register(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType, implementationType);
        }

        public override void RegisterOnlyOnce(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType, implementationType);
        }

        public override void RegisterOpenGeneric(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType.GetGenericTypeDefinition(), implementationType);
        }

        public override void RegisterOpenGenericOnlyOnce(Type serviceType, Type implementationType)
        {
            if (HasMultipleImplementations(serviceType, implementationType))
            {
                return;
            }

            Registrar.Register(serviceType.GetGenericTypeDefinition(), implementationType);
        }

        public override bool IsAlreadyRegistered(Type serviceType, Type implementationType) =>
            throw new NotSupportedException();

        private bool HasMultipleImplementations(Type serviceType, Type implementationType)
        {
            if (!serviceType.IsGenericType)
            {
                return false;
            }
            
            var genericDefinition = serviceType.GetGenericTypeDefinition();
            if (genericDefinition == typeof(INotificationHandler<>))
            {
                _notificationHandlers.Add((serviceType, implementationType));
                return true;
            }

            if (genericDefinition == typeof(IPipelineBehavior<>) || genericDefinition == typeof(IPipelineBehavior<,>) || genericDefinition == typeof(IStreamPipelineBehavior<,>))
            {
                _pipelines.Add((serviceType, implementationType));
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            foreach (var pipelineType in _pipelines.GroupBy(kvp => kvp.ServiceType.GetGenericTypeDefinition()))
            {
                Registrar.Collection.Register(pipelineType.Key.GetGenericTypeDefinition(), pipelineType.Select(kvp => kvp.ImplementationType));
            }

            Registrar.Collection.Register(typeof(INotificationHandler<>), _notificationHandlers.Select(kvp => kvp.ImplementationType));
        }
    }
}