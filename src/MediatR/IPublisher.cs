using MediatR.Examples.PublishStrategies;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR;

/// <summary>
/// Publish a notification or event through the mediator pipeline to be handled by multiple handlers.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    Task Publish(object notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="publishStrategy">Strategy how to publish notification to handlers</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    Task Publish(object notification, PublishStrategy publishStrategy, CancellationToken cancellationToken = default);


    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="publishStrategy">Strategy how to publish notification to handlers</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    Task Publish<TNotification>(TNotification notification, PublishStrategy publishStrategy, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}