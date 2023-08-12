using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;

namespace MediatR.Subscriptions.Notifications;

/// <summary>
/// The abstraction of handling the notification.
/// </summary>
public abstract class NotificationHandler
{
    /// <summary>
    /// Handles the notification with its handlers.
    /// </summary>
    /// <param name="notification">The notification message.</param>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="notificationPublisher">The current notification publisher.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TMethodNotification">The notification type.</typeparam>
    /// <returns>The publishing as a task.</returns>
    public abstract Task Handle<TMethodNotification>(TMethodNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
        where TMethodNotification : INotification;

    /// <summary>
    /// Handles the notification with its handlers.
    /// </summary>
    /// <param name="notification">The notification message.</param>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="notificationPublisher">The current notification publisher.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The publishing as a task.</returns>
    public abstract Task Handle(object notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken);
}