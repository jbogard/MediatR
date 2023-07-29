using System;
using System.Collections.Generic;
using MediatR.Abstraction;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Processors;
using MediatR.DependencyInjection.AssemblyScanner;
using MediatR.DependencyInjection.Configuration;
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

    public static IEnumerable<Type> GetInternalProcessorPipelines((TypeWrapper, AssemblyScannerOptions)[] userImplementedTypes, MediatRServiceConfiguration configuration)
    {
        if (HasRelevantImplementation(userImplementedTypes,
                AssemblyScannerOptions.RequestPreProcessor | AssemblyScannerOptions.RequestPostProcessor,
                (typeof(IRequestPreProcessor<>), configuration.RequestPreProcessors), (typeof(IRequestPostProcessor<>), configuration.RequestPostProcessors)))
            if (configuration.EnableCachingOfHandlers)
                yield return typeof(CachingRequestProcessorBehavior<>);
            else
                yield return typeof(TransientRequestProcessorBehavior<>);

        if (HasRelevantImplementation(userImplementedTypes,
                AssemblyScannerOptions.RequestResponsePreProcessor | AssemblyScannerOptions.RequestResponsePostProcessor,
                (typeof(IRequestPreProcessor<,>), configuration.RequestResponsePreProcessors), (typeof(IRequestPostProcessor<,>), configuration.RequestResponsePostProcessors)))
            if (configuration.EnableCachingOfHandlers)
                yield return typeof(CachingRequestResponseProcessorBehavior<,>);
            else
                yield return typeof(TransientRequestResponseProcessorBehavior<,>);
    }

    public static IEnumerable<Type> GetInternalExceptionHandlingPipelines((TypeWrapper,AssemblyScannerOptions)[] userImplementedTypes, MediatRServiceConfiguration configuration)
    {
        if (configuration.RequestExceptionActionProcessorStrategy == RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions)
        {
            if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestExceptionActionHandler, (typeof(IRequestExceptionAction<,>), null)))
                yield return typeof(RequestExceptionActionProcessorBehavior<>);

            if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestExceptionHandler, (typeof(IRequestExceptionHandler<,>), null)))
                yield return typeof(RequestExceptionHandlerProcessBehavior<>);

            if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestResponseExceptionActionHandler, (typeof(IRequestResponseExceptionAction<,,>), null)))
                yield return typeof(RequestResponseExceptionActionProcessBehavior<,>);

            if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestResponseExceptionHandler, (typeof(IRequestResponseExceptionHandler<,,>), null)))
                yield return typeof(RequestResponseExceptionHandlerProcessBehavior<,>);

            yield break;
        }

        if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestExceptionHandler, (typeof(IRequestExceptionHandler<,>), null)))
            yield return typeof(RequestExceptionHandlerProcessBehavior<>);

        if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestExceptionActionHandler, (typeof(IRequestExceptionAction<,>), null)))
            yield return typeof(RequestExceptionActionProcessorBehavior<>);

        if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestResponseExceptionHandler, (typeof(IRequestResponseExceptionHandler<,,>), null)))
            yield return typeof(RequestResponseExceptionHandlerProcessBehavior<,>);

        if (HasRelevantImplementation(userImplementedTypes, AssemblyScannerOptions.RequestResponseExceptionActionHandler, (typeof(IRequestResponseExceptionAction<,,>), null)))
            yield return typeof(RequestResponseExceptionActionProcessBehavior<,>);
    }

    private static bool HasRelevantImplementation((TypeWrapper, AssemblyScannerOptions)[] userImplementedTypes, AssemblyScannerOptions requiredAssemblyScannerOptions, params (Type OpenGenericInterfaceDefinition, TypeRegistrar? TypeRegistrar)[] handlingOpenGenericInterface)
    {
        foreach (var (typeWrapper, scannerOptions) in userImplementedTypes)
        {
            if (!scannerOptions.HasFlag(requiredAssemblyScannerOptions))
                continue;

            foreach (var (openGenericHandlingInterfaceDefinition, typeRegistrar) in handlingOpenGenericInterface)
            {
                if (typeRegistrar?.Services.Count > 0)
                {
                    return true;
                }

                foreach (var (_, openGenericInterface) in typeWrapper.OpenGenericInterfaces)
                {
                    if (openGenericInterface == openGenericHandlingInterfaceDefinition)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}