using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.Notifications;

internal sealed class CachedNotificationHandler<TNotification> : NotificationHandler
    where TNotification : INotification
{
    private INotificationHandler<TNotification>[]? _cachedHandlers;

    public override Task Handle<TMethodNotification>(TMethodNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TNotification) == typeof(TMethodNotification), "notification type and method notification type must be the same types.");

        return notificationPublisher.Publish((INotificationHandler<TMethodNotification>[]) GetHandlers(serviceProvider), notification, cancellationToken);
    }

    public override Task Handle(object notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
    {
        Debug.Assert(notification.GetType() == typeof(TNotification), "Notification Types must be the same.");

        return notificationPublisher.Publish(GetHandlers(serviceProvider), (TNotification) notification, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private INotificationHandler<TNotification>[] GetHandlers(IServiceProvider serviceProvider) =>
        _cachedHandlers ??= serviceProvider.GetServices<INotificationHandler<TNotification>>();
}