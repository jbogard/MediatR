using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;

namespace MediatR.NotificationPublishers;

/// <summary>
/// Uses Task.WhenAll with the list of Handler tasks:
/// <code>
/// Task.WhenAll(notificationHandlers.Select(h => h.Handle(notification, cancellationToken)));
/// </code>
/// </summary>
public class TaskWhenAllPublisher : INotificationPublisher
{
    public Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification =>
        Task.WhenAll(notificationHandlers.Select(h => h.Handle(notification, cancellationToken)));
}