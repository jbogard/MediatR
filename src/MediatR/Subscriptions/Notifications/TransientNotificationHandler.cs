using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.Notifications;

internal sealed class TransientNotificationHandler<TNotification> : NotificationHandler
    where TNotification : INotification
{
    public override void Handle<TMethodNotification>(TMethodNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TNotification) == typeof(TMethodNotification), "notification type and method notification type must be the same type.");
        notificationPublisher.Publish((INotificationHandler<TMethodNotification>[])GetHandlers(serviceProvider), notification, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static INotificationHandler<TNotification>[] GetHandlers(IServiceProvider serviceProvider) =>
        serviceProvider.GetServices<INotificationHandler<TNotification>>();
}