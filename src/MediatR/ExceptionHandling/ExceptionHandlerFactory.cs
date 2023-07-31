using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MediatR.DependencyInjection.Configuration;
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
    private static Type genericRequestExceptionHandler = typeof(TransientRequestExceptionRequestHandler<,>);
    private static Type genericRequestResponseExceptionActionHandler = typeof(TransientRequestResponseExceptionActionHandler<,,>);
    private static Type genericRequestResponseExceptionHandler = typeof(TransientRequestResponseExceptionHandler<,,>);

    [ThreadStatic]
    private static Type[]? genericRequestTypeCache;

    [ThreadStatic]
    private static Type[]? genericHandlerTypeCache;

    public static void Initialize(MediatRServiceConfiguration configuration)
    {
        if (configuration.EnableCachingOfHandlers)
        {
            genericRequestExceptionActionHandler = typeof(CachedRequestExceptionActionHandler<,>);
            genericRequestExceptionHandler = typeof(CachedRequestExceptionHandler<,>);
            genericRequestResponseExceptionActionHandler = typeof(CachedRequestResponseExceptionActionHandler<,,>);
            genericRequestResponseExceptionHandler = typeof(CachedRequestResponseExceptionHandler<,,>);
        }
    }

    public static RequestExceptionActionHandler CreateRequestExceptionActionHandler(Type requestType, Type exceptionType) =>
        RequestExceptionActionHandlers.GetOrAdd((requestType, exceptionType), RequestActionHandlerFactory);

    private static RequestExceptionActionHandler RequestActionHandlerFactory((Type RequestType, Type ExceptionType) tuple)
    {
        TryInitGenericHandlerTypeCache();
        genericHandlerTypeCache![0] = tuple.RequestType;
        genericHandlerTypeCache[1] = tuple.ExceptionType;
        var requestExceptionActionHandler = genericRequestExceptionActionHandler.MakeGenericType(genericHandlerTypeCache);

        return (RequestExceptionActionHandler) Activator.CreateInstance(requestExceptionActionHandler)!;
    }

    public static RequestExceptionRequestHandler CreateRequestExceptionRequestHandler(Type requestType, Type exceptionType) =>
        RequestExceptionRequestHandlers.GetOrAdd((requestType, exceptionType), RequestHandlerFactory);

    private static RequestExceptionRequestHandler RequestHandlerFactory((Type RequestType, Type ExceptionType) tuple)
    {
        TryInitGenericHandlerTypeCache();
        genericHandlerTypeCache![0] = tuple.RequestType;
        genericHandlerTypeCache[1] = tuple.ExceptionType;
        var requestExceptionHandler = genericRequestExceptionHandler.MakeGenericType(genericHandlerTypeCache);

        return (RequestExceptionRequestHandler) Activator.CreateInstance(requestExceptionHandler)!;
    }

    public static RequestResponseExceptionActionHandler CreateRequestResponseExceptionActionHandler(Type requestType, Type responseType, Type exceptionType) =>
        RequestResponseExceptionActionHandlers.GetOrAdd((requestType, responseType, exceptionType), RequestResponseActionHandlerFactory);

    private static RequestResponseExceptionActionHandler RequestResponseActionHandlerFactory((Type RequestType, Type ResponseType, Type ExceptionType) tuple)
    {
        TryInitGenericRequestTypeCache();
        genericRequestTypeCache![0] = tuple.RequestType;
        genericRequestTypeCache[1] = tuple.ResponseType;
        genericRequestTypeCache[2] = tuple.ExceptionType;
        var requestResponseExceptionActionHandler = genericRequestResponseExceptionActionHandler.MakeGenericType(genericRequestTypeCache);

        return (RequestResponseExceptionActionHandler) Activator.CreateInstance(requestResponseExceptionActionHandler);
    }

    public static RequestResponseExceptionRequestHandler CreateRequestResponseExceptionRequestHandler(Type requestType, Type responseType, Type exceptionType) =>
        RequestResponseExceptionRequestHandlers.GetOrAdd((requestType, responseType, exceptionType), RequestResponseHandlerFactory);

    private static RequestResponseExceptionRequestHandler RequestResponseHandlerFactory((Type RequestType, Type ResponseType, Type ExceptionType) tuple)
    {
        TryInitGenericRequestTypeCache();
        genericRequestTypeCache![0] = tuple.RequestType;
        genericRequestTypeCache[1] = tuple.ResponseType;
        genericRequestTypeCache[2] = tuple.ExceptionType;
        var requestResponseExceptionHandler = genericRequestResponseExceptionHandler.MakeGenericType(genericRequestTypeCache);

        return (RequestResponseExceptionRequestHandler) Activator.CreateInstance(requestResponseExceptionHandler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitGenericRequestTypeCache() =>
        genericRequestTypeCache ??= new Type[3];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitGenericHandlerTypeCache() =>
        genericHandlerTypeCache ??= new Type[2];
}