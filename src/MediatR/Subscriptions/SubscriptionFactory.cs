using System;
using System.Runtime.CompilerServices;
using MediatR.Abstraction;

namespace MediatR.Subscriptions;

internal sealed class SubscriptionFactory
{
    private static readonly Type GenericNotificationHandlerType = typeof(NotificationHandler<>);
    private static readonly Type GenericRequestHandlerType = typeof(RequestHandler<>);
    private static readonly Type GenericRequestResponseHandlerType = typeof(RequestResponseHandler<,>);
    private static readonly Type GenericStreamRequestHandlerType = typeof(StreamRequestHandler<,>);

    [ThreadStatic]
    private static Type[]? GenericRequestTypeCache;

    [ThreadStatic]
    private static Type[]? GenericHandlerTypeCache;

    private readonly object[] _creationArgs;
    private readonly object[] _notificationArgs;

    public SubscriptionFactory(IServiceProvider serviceProvider, INotificationPublisher notificationPublisher)
    {
        _creationArgs = new object[] { serviceProvider };
        _notificationArgs = new object[] { serviceProvider, notificationPublisher };
    }

    public NotificationHandler CreateNotificationHandler(Type notificationType)
    {
        TryInitHandlerThreadStaticTypeCache();
        GenericHandlerTypeCache![0] = notificationType;
        var notificationHandlerType = GenericNotificationHandlerType.MakeGenericType(GenericHandlerTypeCache);

        return (NotificationHandler) Activator.CreateInstance(notificationHandlerType, _notificationArgs)!;
    }

    public RequestHandler CreateRequestHandler(Type requestType)
    {
        TryInitHandlerThreadStaticTypeCache();
        GenericHandlerTypeCache![0] = requestType;
        var requestHandlerType = GenericRequestHandlerType.MakeGenericType(GenericHandlerTypeCache);

        return (RequestHandler) Activator.CreateInstance(requestHandlerType, _creationArgs)!;
    }

    public RequestResponseHandler CreateRequestResponseHandler(Type requestType, Type responseType)
    {
        TryInitRequestThreadStaticTypeCache();
        GenericRequestTypeCache![0] = requestType;
        GenericRequestTypeCache[1] = responseType;
        var requestResponseType = GenericRequestResponseHandlerType.MakeGenericType(GenericRequestTypeCache);

        return (RequestResponseHandler) Activator.CreateInstance(requestResponseType, _creationArgs)!;
    }

    public StreamRequestHandler CreateStreamRequestHandler(Type requestType, Type responseType)
    {
        TryInitRequestThreadStaticTypeCache();
        GenericRequestTypeCache![0] = requestType;
        GenericRequestTypeCache[1] = responseType;
        var streamRequestType = GenericStreamRequestHandlerType.MakeGenericType(GenericRequestTypeCache);

        return (StreamRequestHandler) Activator.CreateInstance(streamRequestType, _creationArgs)!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitRequestThreadStaticTypeCache() => GenericRequestTypeCache ??= new Type[2];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitHandlerThreadStaticTypeCache() => GenericHandlerTypeCache ??= new Type[1];
}