using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines a cancellable, asynchronous handler for a notification
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled</typeparam>
    public interface ICancellableAsyncNotificationHandler<in TNotification>
        where TNotification : ICancellableAsyncNotification
    {
        /// <summary>
        /// Handles a cancellable, asynchronous notification
        /// </summary>
        /// <param name="notification">The notification message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing handling the notification</returns>
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }
}