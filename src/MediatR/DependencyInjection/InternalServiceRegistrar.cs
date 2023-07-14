using System;
using MediatR.Abstraction;
using MediatR.ExceptionHandling;
using MediatR.Pipeline.Request;
using MediatR.Pipeline.RequestResponse;
using MediatR.Subscriptions;

namespace MediatR.DependencyInjection;

internal static class InternalServiceRegistrar
{
    public static void AddInternalServiceTypes<TRegistrar>(MediatRServiceConfiguration<TRegistrar> configuration)
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

    public static Type[] GetInternalProcessorPipelines() =>
        new[]
        {
            typeof(RequestProcessorBehavior<>),
            typeof(RequestResponseProcessorBehavior<,>)
        };

    public static Type[] GetInternalExceptionHandlingPipelines<TRegistrar>(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        if (configuration.RequestExceptionActionProcessorStrategy == RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions)
        {
            return new[]
            {
                typeof(RequestExceptionActionProcessorBehavior<>),
                typeof(RequestExceptionHandlerProcessBehavior<>),
                typeof(RequestResponseExceptionActionProcessBehavior<,>),
                typeof(RequestResponseExceptionRequestHandlerProcessBehavior<,>)
            };
        }

        return new[]
        {
            typeof(RequestExceptionHandlerProcessBehavior<>),
            typeof(RequestExceptionActionProcessorBehavior<>),
            typeof(RequestResponseExceptionRequestHandlerProcessBehavior<,>),
            typeof(RequestResponseExceptionActionProcessBehavior<,>)
        };
    }
}