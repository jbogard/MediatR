using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;
using MediatR.Subscriptions.Notifications;

namespace MediatR.NotificationPublishers;

/// <summary>
/// Awaits each notification handler in a single foreach loop:
/// <code>
/// foreach (var handler in handlers)
///     await handler(notification, cancellationToken);
/// </code>
/// <remarks>
/// With the ForeachAwaitPublisher the caller of IMediator.Publish will NOT wait until all notification handlers have been executed.
/// </remarks>
/// </summary>
public class ForeachAwaitPublisher : INotificationPublisher
{
    /// <inheritdoc />
    public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
        where TNotification : INotification =>
        notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken);

    /// <inheritdoc />
    public async Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        foreach (var notificationHandler in notificationHandlers)
        {
            await notificationHandler.Handle(notification, cancellationToken);
        }
    }
}