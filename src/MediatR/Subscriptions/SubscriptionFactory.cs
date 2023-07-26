using System;
using System.Runtime.CompilerServices;
using MediatR.DependencyInjection.ConfigurationBase;
using MediatR.Subscriptions.Notifications;
using MediatR.Subscriptions.Requests;
using MediatR.Subscriptions.StreamingRequests;

namespace MediatR.Subscriptions;

internal static class SubscriptionFactory
{
    private static Type genericNotificationHandlerType = typeof(TransientNotificationHandler<>);
    private static Type genericRequestHandlerType = typeof(TransientRequestHandler<>);
    private static Type genericRequestResponseHandlerType = typeof(TransientRequestResponseHandler<,>);
    private static Type genericStreamRequestHandlerType = typeof(TransientStreamRequestHandler<,>);

    [ThreadStatic]
    private static Type[]? genericRequestTypeCache;

    [ThreadStatic]
    private static Type[]? genericHandlerTypeCache;

    public static void Initialize(MediatRServiceConfiguration configuration)
    {
        if (configuration.EnableCachingOfHandlers)
        {
            genericNotificationHandlerType = typeof(CachedNotificationHandler<>);
            genericRequestHandlerType = typeof(CachedRequestHandler<>);
            genericRequestResponseHandlerType = typeof(CachedRequestResponseHandler<,>);
            genericStreamRequestHandlerType = typeof(CachedStreamRequestHandler<,>);
        }
    }

    public static NotificationHandler CreateNotificationHandler(Type notificationType)
    {
        TryInitHandlerThreadStaticTypeCache();
        genericHandlerTypeCache![0] = notificationType;
        var notificationHandlerType = genericNotificationHandlerType.MakeGenericType(genericHandlerTypeCache);

        return (NotificationHandler) Activator.CreateInstance(notificationHandlerType)!;
    }

    public static RequestHandler CreateRequestHandler(Type requestType)
    {
        TryInitHandlerThreadStaticTypeCache();
        genericHandlerTypeCache![0] = requestType;
        var requestHandlerType = genericRequestHandlerType.MakeGenericType(genericHandlerTypeCache);

        return (RequestHandler) Activator.CreateInstance(requestHandlerType)!;
    }

    public static RequestResponseHandler CreateRequestResponseHandler(Type requestType, Type responseType)
    {
        TryInitRequestThreadStaticTypeCache();
        genericRequestTypeCache![0] = requestType;
        genericRequestTypeCache[1] = responseType;
        var requestResponseType = genericRequestResponseHandlerType.MakeGenericType(genericRequestTypeCache);

        return (RequestResponseHandler) Activator.CreateInstance(requestResponseType)!;
    }

    public static StreamRequestHandler CreateStreamRequestHandler(Type requestType, Type responseType)
    {
        TryInitRequestThreadStaticTypeCache();
        genericRequestTypeCache![0] = requestType;
        genericRequestTypeCache[1] = responseType;
        var streamRequestType = genericStreamRequestHandlerType.MakeGenericType(genericRequestTypeCache);

        return (StreamRequestHandler) Activator.CreateInstance(streamRequestType)!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitRequestThreadStaticTypeCache() =>
        genericRequestTypeCache ??= new Type[2];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TryInitHandlerThreadStaticTypeCache() =>
        genericHandlerTypeCache ??= new Type[1];
}