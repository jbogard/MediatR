using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Abstraction;

/// <summary>
/// Defines how to publish the notification with the handlers.
/// </summary>
public interface INotificationPublisher
{
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