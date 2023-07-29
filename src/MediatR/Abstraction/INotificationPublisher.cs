using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;
using MediatR.Subscriptions.Notifications;

namespace MediatR.Abstraction;

/// <summary>
/// Defines how to publish the notification with the handlers.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    ///  Defines how to handle the publishing task.
    /// </summary>
    /// <param name="notificationHandler">The notification handler.</param>
    /// <param name="notification">The notification message.</param>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="notificationPublisher">The current notification publisher.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
        where TNotification : INotification;
    
    /// <summary>
    /// Publishes the <paramref name="notification"/> to the handlers <paramref name="notificationHandlers"/>.
    /// </summary>
    /// <param name="notificationHandlers">All notification handlers.</param>
    /// <param name="notification">The notification message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <returns>The publishing action as a task.</returns>
    Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification;
}