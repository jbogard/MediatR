using System.Threading;

namespace MediatR.Abstraction;

/// <summary>
/// Publish a notification or event through the mediator pipeline to be handled by multiple handlers.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Sends a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    void Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Sends a notification to multiple handlers via dynamic dispatching.
    /// </summary>
    /// <param name="notification">The notification object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    void Publish(object? notification, CancellationToken cancellationToken = default);
}