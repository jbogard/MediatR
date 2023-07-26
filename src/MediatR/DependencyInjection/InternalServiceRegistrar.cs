using System;
using System.Collections.Generic;
using MediatR.Abstraction;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Processors;
using MediatR.DependencyInjection.AssemblyScanner;
using MediatR.DependencyInjection.ConfigurationBase;
using MediatR.ExceptionHandling.Request;
using MediatR.ExceptionHandling.RequestResponse;
using MediatR.Pipeline.Request;
using MediatR.Pipeline.RequestResponse;

namespace MediatR.DependencyInjection;

internal static class InternalServiceRegistrar
{
    public static void RegisterInternalServiceTypes<TRegistrar, TConfiguration>(
        DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter,
        MediatRServiceConfiguration configuration)
        where TConfiguration : MediatRServiceConfiguration
    {
        adapter.Register(configuration, typeof(Mediator), (typeof(IMediator), true), (typeof(ISender), true), (typeof(IPublisher), true));

        if (configuration.NotificationPublisherType is not null)
            adapter.Register(configuration, configuration.NotificationPublisherType, (typeof(INotificationPublisher), true));
        else
            adapter.RegisterInstance(typeof(INotificationPublisher), configuration.NotificationPublisher);
    }

    public static IEnumerable<Type> GetInternalProcessorPipelines(TypeWrapper[] typeWrappers, IComparer<Type> comparer, MediatRServiceConfiguration configuration)
    {
        if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestPreProcessor<>), typeof(IRequestPostProcessor<>)))
            if (configuration.EnableCachingOfHandlers)
                yield return typeof(CachingRequestProcessorBehavior<>);
            else
                yield return typeof(TransientRequestProcessorBehavior<>);

        if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestPreProcessor<,>), typeof(IRequestPostProcessor<,>)))
            if (configuration.EnableCachingOfHandlers)
                yield return typeof(CachingRequestResponseProcessorBehavior<,>);
            else
                yield return typeof(TransientRequestResponseProcessorBehavior<,>);
    }

    public static IEnumerable<Type> GetInternalExceptionHandlingPipelines(TypeWrapper[] typeWrappers, IComparer<Type> comparer, MediatRServiceConfiguration configuration)
    {
        if (configuration.RequestExceptionActionProcessorStrategy == RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions)
        {
            if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestExceptionAction<,>)))
                yield return typeof(RequestExceptionActionProcessorBehavior<>);

            if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestExceptionHandler<,>)))
                yield return typeof(RequestExceptionHandlerProcessBehavior<>);

            if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestResponseExceptionAction<,,>)))
                yield return typeof(RequestResponseExceptionActionProcessBehavior<,>);

            if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestResponseExceptionHandler<,,>)))
                yield return typeof(RequestResponseExceptionRequestHandlerProcessBehavior<,>);

            yield break;
        }

        if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestExceptionHandler<,>)))
            yield return typeof(RequestExceptionHandlerProcessBehavior<>);

        if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestExceptionAction<,>)))
            yield return typeof(RequestExceptionActionProcessorBehavior<>);

        if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestResponseExceptionHandler<,,>)))
            yield return typeof(RequestResponseExceptionRequestHandlerProcessBehavior<,>);

        if (HasRelevantImplementation(typeWrappers, comparer, typeof(IRequestResponseExceptionAction<,,>)))
            yield return typeof(RequestResponseExceptionActionProcessBehavior<,>);
    }

    private static bool HasRelevantImplementation(TypeWrapper[] wrappers, IComparer<Type> comparer, params Type[] handlingOpenGenericInterface)
    {
        foreach (var typeWrapper in wrappers)
        {
            foreach (var (_, openGenericInterface) in typeWrapper.OpenGenericInterfaces)
            {
                if (Array.BinarySearch(handlingOpenGenericInterface, openGenericInterface, comparer) > -1)
                {
                    return true;
                }
            }
        }

        return false;
    }
}