using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MediatR.DependencyInjection.ConfigurationBase;
using MediatR.ExceptionHandling.Request.Subscription;
using MediatR.ExceptionHandling.RequestResponse.Subscription;

namespace MediatR.ExceptionHandling;

internal static class ExceptionHandlerFactory
{
    private static readonly ConcurrentDictionary<(Type RequestType, Type ExceptionType), RequestExceptionActionHandler> RequestExceptionActionHandlers = new();
    private static readonly ConcurrentDictionary<(Type RequestType, Type ExceptionType), RequestExceptionRequestHandler> RequestExceptionRequestHandlers = new();
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType, Type ExceptionType), RequestResponseExceptionActionHandler> RequestResponseExceptionActionHandlers = new();
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType, Type ExceptionType), RequestResponseExceptionRequestHandler> RequestResponseExceptionRequestHandlers = new();

    private static Type genericRequestExceptionActionHandler = typeof(TransientRequestExceptionActionHandler<,>);
    private static Type genericRequestExceptionRequestHandler = typeof(TransientRequestExceptionRequestHandler<,>);
    private static Type genericRequestResponseExceptionActionHandler = typeof(TransientRequestResponseExceptionActionHandler<,,>);
    private static Type genericRequestResponseExceptionRequestHandler = typeof(TransientRequestResponseExceptionRequestHandler<,,>);

    [ThreadStatic]
    private static Type[]? GenericRequestTypeCache;

    [ThreadStatic]
    private static Type[]? GenericHandlerTypeCache;

    public static void Initialize(MediatRServiceConfiguration configuration)
    {
        if (configuration.EnableCachingOfHandlers)
        {
            genericRequestExceptionActionHandler = typeof(CachedRequestExceptionActionHandler<,>);
            genericRequestExceptionRequestHandler = typeof(CachedRequestExceptionRequestHandler<,>);
            genericRequestResponseExceptionActionHandler = typeof(CachedRequestResponseExceptionActionHandler<,,>);
            genericRequestResponseExceptionRequestHandler = typeof(CachedRequestResponseExceptionRequestHandler<,,>);
        }
    }

    public static RequestExceptionActionHandler CreateRequestExceptionActionHandler(Type requestType, Type exceptionType) =>
        RequestExceptionActionHandlers.GetOrAdd((requestType, exceptionType), RequestActionHandlerFactory);

    private static RequestExceptionActionHandler RequestActionHandlerFactory((Type RequestType, Type ExceptionType) tuple)
    {
        TryInitGenericHandlerTypeCache();
        GenericHandlerTypeCache![0] = tuple.RequestType;
        GenericHandlerTypeCache[1] = tuple.ExceptionType;
        var requestExceptionActionHandler = genericRequestExceptionActionHandler.MakeGenericType(GenericHandlerTypeCache);

        return (RequestExceptionActionHandler) Activator.CreateInstance(requestExceptionActionHandler)!;
    }

    public static RequestExceptionRequestHandler CreateRequestExceptionRequestHandler(Type requestType, Type exceptionType) =>
        RequestExceptionRequestHandlers.GetOrAdd((requestType, exceptionType), RequestHandlerFactory);

    private static RequestExceptionRequestHandler RequestHandlerFactory((Type RequestType, Type ExceptionType) tuple)
    {
        TryInitGenericHandlerTypeCache();
        GenericHandlerTypeCache![0] = tuple.RequestType;
        GenericHandlerTypeCache[1] = tuple.ExceptionType;
        var requestExceptionHandler = genericRequestExceptionRequestHandler.MakeGenericType(GenericHandlerTypeCache);

        return (RequestExceptionRequestHandler) Activator.CreateInstance(requestExceptionHandler)!;
    }

    public static RequestResponseExceptionActionHandler CreateRequestResponseExceptionActionHandler(Type requestType, Type responseType, Type exceptionType) =>
        RequestResponseExceptionActionHandlers.GetOrAdd((requestType, responseType, exceptionType), RequestResponseActionHandlerFactory);

    private static RequestResponseExceptionActionHandler RequestResponseActionHandlerFactory((Type RequestType, Type ResponseType, Type ExceptionType) tuple)
    {
        TryInitGenericRequestTypeCache();
        GenericRequestTypeCache![0] = tuple.RequestType;
        GenericRequestTypeCache[1] = tuple.ResponseType;
        GenericRequestTypeCache[2] = tuple.ExceptionType;
        var requestResponseExceptionActionHandler = genericRequestResponseExceptionActionHandler.MakeGenericType(GenericRequestTypeCache);

        return (RequestResponseExceptionActionHandler) Activator.CreateInstance(requestResponseExceptionActionHandler);
    }

    public static RequestResponseExceptionRequestHandler CreateRequestResponseExceptionRequestHandler(Type requestType, Type responseType, Type exceptionType) =>
        RequestResponseExceptionRequestHandlers.GetOrAdd((requestType, responseType, exceptionType), RequestResponseHandlerFactory);

    private static RequestResponseExceptionRequestHandler RequestResponseHandlerFactory((Type RequestType, Type ResponseType, Type ExceptionType) tuple)
    {
        TryInitGenericRequestTypeCache();
        GenericRequestTypeCache![0] = tuple.RequestType;
        GenericRequestTypeCache[1] = tuple.ResponseType;
        GenericRequestTypeCache[2] = tuple.ExceptionType;
        var requestResponseExceptionHandler = genericRequestResponseExceptionRequestHandler.MakeGenericType(GenericRequestTypeCache);

        return (RequestResponseExceptionRequestHandler) Activator.CreateInstance(requestResponseExceptionHandler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitGenericRequestTypeCache() =>
        GenericRequestTypeCache ??= new Type[3];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitGenericHandlerTypeCache() =>
        GenericHandlerTypeCache ??= new Type[2];
}