using System;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions;

internal sealed class NotificationHandler<TNotification> : NotificationHandler
    where TNotification : INotification
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationPublisher _notificationPublisher;

    public NotificationHandler(IServiceProvider serviceProvider, INotificationPublisher notificationPublisher)
    {
        _serviceProvider = serviceProvider;
        _notificationPublisher = notificationPublisher;
    }

    public override void Handle(INotification notification, CancellationToken cancellationToken) =>
        _notificationPublisher.Publish(GetHandlers(), (TNotification)notification, cancellationToken).GetAwaiter().GetResult();

    // Don't cache the handler because more handlers could be added during runtime.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private INotificationHandler<TNotification>[] GetHandlers() => _serviceProvider.GetServices<INotificationHandler<TNotification>>();
}