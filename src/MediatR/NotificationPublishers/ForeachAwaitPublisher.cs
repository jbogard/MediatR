using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;

namespace MediatR.NotificationPublishers;

/// <summary>
/// Awaits each notification handler in a single foreach loop:
/// <code>
/// foreach (var handler in handlers) {
///     await handler(notification, cancellationToken);
/// }
/// </code>
/// </summary>
public class ForeachAwaitPublisher : INotificationPublisher
{
    public async Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        foreach (var notificationHandler in notificationHandlers)
        {
            await notificationHandler.Handle(notification, cancellationToken);
        }
    }
}