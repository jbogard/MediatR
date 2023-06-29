using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System;
using MediatR.DependencyInjection;
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

        container.Register(() => (TextWriter) writer, Lifestyle.Singleton);

        //Pipeline
        container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
        {
            typeof(RequestExceptionProcessorBehavior<,>),
            typeof(RequestExceptionActionProcessorBehavior<,>),
            typeof(RequestPreProcessorBehavior<,>),
            typeof(RequestPostProcessorBehavior<,>),
            typeof(GenericPipelineBehavior<,>)
        });
        container.Collection.Register(typeof(IRequestPreProcessor<>), new[] { typeof(GenericRequestPreProcessor<>) });
        container.Collection.Register(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>) });
        container.Collection.Register(typeof(IStreamPipelineBehavior<,>), new[]
        {
            typeof(GenericStreamPipelineBehavior<,>)
        });

        services.BuildServiceProvider().UseSimpleInjector(container);

        container.RegisterInstance<IServiceProvider>(container);

        var mediator = container.GetRequiredService<IMediator>();

        return mediator;
    }
}

internal static class ContainerExtension
{
    public static Container ConfigureMediarR(this Container containerInstance, Action<MediatRServiceConfiguration<Container>> configuration)
    {
        var dependencyRegistrarationConfiguration = new DependencyInjectionRegistrarAdapter<Container>(
            containerInstance,
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Singleton),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Singleton),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Singleton),
            (container, serviceType, implementationType) => container.Register(serviceType, implementationType, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Transient),
            (container, serviceType, implementationType) => container.Register(serviceType, new[] {implementationType}, Lifestyle.Singleton),
            (container, fromType, toType) => container.Register(toType, fromType, Lifestyle.Singleton),
            (container, serviceType, instance) => container.RegisterInstance(serviceType, instance));

        MediatRConfigurator.ConfigureMediatR(dependencyRegistrarationConfiguration, configuration);

        return containerInstance;
    }
}