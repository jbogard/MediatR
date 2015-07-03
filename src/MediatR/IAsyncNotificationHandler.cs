using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines an asynchronous handler for a notification
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled</typeparam>
    public interface IAsyncNotificationHandler<in TNotification>
        where TNotification : IAsyncNotification
    {
        /// <summary>
        /// Handles an asynchronous notification
        /// </summary>
        /// <param name="notification">The notification message</param>
        /// <returns>A task representing handling the notification</returns>
        Task Handle(TNotification notification);
    }
}