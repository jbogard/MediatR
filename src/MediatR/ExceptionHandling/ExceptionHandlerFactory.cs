using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace MediatR.ExceptionHandling;

internal sealed class ExceptionHandlerFactory
{
    private static readonly Type GenericRequestExceptionActionHandler = typeof(RequestExceptionActionHandler<,>);
    private static readonly Type GenericRequestExceptionRequestHandler = typeof(RequestExceptionRequestHandler<,>);
    private static readonly Type GenericRequestResponseExceptionActionHandler = typeof(RequestResponseExceptionActionHandler<,,>);
    private static readonly Type GenericRequestResponseExceptionRequestHandler = typeof(RequestResponseExceptionRequestHandler<,,>);

    private static readonly ConcurrentDictionary<(Type RequestType, Type ExceptionType), RequestExceptionActionHandler> RequestExceptionActionHandlers = new();
    private static readonly ConcurrentDictionary<(Type RequestType, Type ExceptionType), RequestExceptionRequestHandler> RequestExceptionRequestHandlers = new();
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType, Type ExceptionType), RequestResponseExceptionActionHandler> RequestResponseExceptionActionHandlers = new();
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType, Type ExceptionType), RequestResponseExceptionRequestHandler> RequestResponseExceptionRequestHandlers = new();

    [ThreadStatic]
    private static Type[]? GenericRequestTypeCache;

    [ThreadStatic]
    private static Type[]? GenericHandlerTypeCache;

    private readonly Func<(Type RequestType, Type ExceptionType), RequestExceptionActionHandler> _requestActionHandlerFactory;
    private readonly Func<(Type RequestType, Type ExceptionType), RequestExceptionRequestHandler> _requestHandlerfactory;
    private readonly Func<(Type RequestType, Type ResponseType, Type ExceptionType), RequestResponseExceptionActionHandler> _requestResponseActionHandlerFactory;
    private readonly Func<(Type RequestType, Type ResponseType, Type ExceptionType), RequestResponseExceptionRequestHandler> _requestResponseHandlerFactory;

    public ExceptionHandlerFactory(IServiceProvider serviceProvider)
    {
        var creationArgs = new object[] {serviceProvider};

        _requestActionHandlerFactory = tuple =>
        {
            TryInitGenericHandlerTypeCache();
            GenericHandlerTypeCache![0] = tuple.RequestType;
            GenericHandlerTypeCache[1] = tuple.ExceptionType;
            var genericRequestExceptionHandler = GenericRequestExceptionActionHandler.MakeGenericType(GenericHandlerTypeCache);

            return (RequestExceptionActionHandler) Activator.CreateInstance(genericRequestExceptionHandler, creationArgs)!;
        };

        _requestHandlerfactory = tuple =>
        {
            TryInitGenericHandlerTypeCache();
            GenericHandlerTypeCache![0] = tuple.RequestType;
            GenericHandlerTypeCache[1] = tuple.ExceptionType;
            var genericRequestExceptionHandler = GenericRequestExceptionRequestHandler.MakeGenericType(GenericHandlerTypeCache);

            return (RequestExceptionRequestHandler) Activator.CreateInstance(genericRequestExceptionHandler, creationArgs)!;
        };

        _requestResponseActionHandlerFactory = tuple =>
        {
            TryInitGenericRequestTypeCache();
            GenericRequestTypeCache![0] = tuple.RequestType;
            GenericRequestTypeCache[1] = tuple.ResponseType;
            GenericRequestTypeCache[2] = tuple.ExceptionType;
            var genericRequestResponseExceptionActionHandler = GenericRequestResponseExceptionActionHandler.MakeGenericType(GenericRequestTypeCache);

            return (RequestResponseExceptionActionHandler) Activator.CreateInstance(genericRequestResponseExceptionActionHandler, creationArgs);
        };

        _requestResponseHandlerFactory = tuple =>
        {
            TryInitGenericRequestTypeCache();
            GenericRequestTypeCache![0] = tuple.RequestType;
            GenericRequestTypeCache[1] = tuple.ResponseType;
            GenericRequestTypeCache[2] = tuple.ExceptionType;
            var genericRequestResponseExceptionHandler = GenericRequestResponseExceptionRequestHandler.MakeGenericType(GenericRequestTypeCache);

            return (RequestResponseExceptionRequestHandler) Activator.CreateInstance(genericRequestResponseExceptionHandler, creationArgs);
        };
    }

    public RequestExceptionActionHandler CreateRequestExceptionActionHandler(Type requestType, Type exceptionType) =>
        RequestExceptionActionHandlers.GetOrAdd((requestType, exceptionType), _requestActionHandlerFactory);

    public RequestExceptionRequestHandler CreateRequestExceptionRequestHandler(Type requestType, Type exceptionType) =>
        RequestExceptionRequestHandlers.GetOrAdd((requestType, exceptionType), _requestHandlerfactory);

    public RequestResponseExceptionActionHandler CreateRequestResponseExceptionActionHandler(Type requestType, Type responseType, Type exceptionType) =>
        RequestResponseExceptionActionHandlers.GetOrAdd((requestType, responseType, exceptionType), _requestResponseActionHandlerFactory);

    public RequestResponseExceptionRequestHandler CreateRequestResponseExceptionRequestHandler(Type requestType, Type responseType, Type exceptionType) =>
        RequestResponseExceptionRequestHandlers.GetOrAdd((requestType, responseType, exceptionType), _requestResponseHandlerFactory);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitGenericRequestTypeCache() =>
        GenericRequestTypeCache ??= new Type[3];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitGenericHandlerTypeCache() =>
        GenericHandlerTypeCache ??= new Type[2];
}