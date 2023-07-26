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