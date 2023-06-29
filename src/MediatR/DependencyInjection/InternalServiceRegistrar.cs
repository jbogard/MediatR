using MediatR.Abstraction;
using MediatR.Abstraction.Behaviors;
using MediatR.ExceptionHandling;
using MediatR.Pipeline.Request;
using MediatR.Pipeline.RequestResponse;
using MediatR.Subscriptions;

namespace MediatR.DependencyInjection;

internal static class InternalServiceRegistrar
{
    public static void AddInternals<TRegistrar>(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        var adapter = configuration.DependencyInjectionRegistrarAdapter;

        adapter.RegisterSingleton(typeof(Mediator), new[]{ typeof(IMediator), typeof(ISender), typeof(IPublisher) });

        if (configuration.NotificationPublisherType is not null)
        {
            adapter.RegisterSingletonOnlyOnce(adapter.Registrar, typeof(INotificationPublisher), configuration.NotificationPublisherType);
        }
        else
        {
            adapter.RegisterInstance(adapter.Registrar, typeof(INotificationPublisher), configuration.NotificationPublisher);
        }

        adapter.RegisterSelfSingletonOnlyOnce(adapter.Registrar, typeof(ExceptionHandlerFactory));
        adapter.RegisterSelfSingletonOnlyOnce(adapter.Registrar, typeof(SubscriptionFactory));
    }

    public static void RegisterInternalProcessorPipelines<TRegistrar>(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        var adapter = configuration.DependencyInjectionRegistrarAdapter;
        
        adapter.Register(configuration, typeof(RequestProcessorBehavior<>), new []{ typeof(IPipelineBehavior<>) }, false);
        adapter.Register(configuration, typeof(RequestResponseProcessorBehavior<,>), new []{ typeof(IPipelineBehavior<,>) }, false);
    }

    public static void RegisterInternalExceptionHandlingPipelines<TRegistrar>(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        var adapter = configuration.DependencyInjectionRegistrarAdapter;
        
        if (configuration.RequestExceptionActionProcessorStrategy == RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions)
        {
            adapter.Register(configuration, typeof(RequestExceptionActionProcessorBehavior<>), new []{ typeof(IPipelineBehavior<>) }, false);
            adapter.Register(configuration, typeof(RequestExceptionHandlerProcessBehavior<>), new []{ typeof(IPipelineBehavior<>) }, false);
            adapter.Register(configuration, typeof(RequestResponseExceptionActionProcessBehavior<,>), new []{ typeof(IPipelineBehavior<,>) }, false);
            adapter.Register(configuration, typeof(RequestResponseExceptionRequestHandlerProcessBehavior<,>), new []{ typeof(IPipelineBehavior<,>) }, false);
        }
        else
        {
            adapter.Register(configuration, typeof(RequestExceptionHandlerProcessBehavior<>), new[] {typeof(IPipelineBehavior<>)}, false);
            adapter.Register(configuration, typeof(RequestExceptionActionProcessorBehavior<>), new[] {typeof(IPipelineBehavior<>)}, false);
            adapter.Register(configuration, typeof(RequestResponseExceptionRequestHandlerProcessBehavior<,>), new[] {typeof(IPipelineBehavior<,>)}, false);
            adapter.Register(configuration, typeof(RequestResponseExceptionActionProcessBehavior<,>), new[] {typeof(IPipelineBehavior<,>)}, false);
        }
    }
}