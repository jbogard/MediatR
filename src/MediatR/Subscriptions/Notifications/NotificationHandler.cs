using System;
using System.Threading;
using MediatR.Abstraction;

namespace MediatR.Subscriptions.Notifications;

internal abstract class NotificationHandler
{
    public abstract void Handle<TMethodNotification>(TMethodNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
        where TMethodNotification : INotification;
}