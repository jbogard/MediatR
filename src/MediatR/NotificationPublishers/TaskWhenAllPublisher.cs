using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;
using MediatR.Subscriptions.Notifications;

namespace MediatR.NotificationPublishers;

/// <summary>
/// Uses Task.WhenAll with the list of Handler tasks:
/// <code>
/// Task.WhenAll(notificationHandlers.Select(h => h.Handle(notification, cancellationToken)));
/// </code>
/// <remarks>
/// With the TaskWhenAllPublisher the caller of IMediator.Publish will wait until all notification handlers have been executed.
/// </remarks>
/// </summary>
public class TaskWhenAllPublisher : INotificationPublisher
{
    /// <inheritdoc />
    public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
        where TNotification : INotification =>
        notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var tasks = new Task[notificationHandlers.Length];

        for (var i = 0; i < notificationHandlers.Length; i++)
        {
            tasks[i] = notificationHandlers[i].Handle(notification, cancellationToken);
        }

        return Task.WhenAll(tasks);
    }
}