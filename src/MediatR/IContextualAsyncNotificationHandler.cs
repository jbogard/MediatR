using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines a context supported, asynchronous handler for a notification
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled</typeparam>
    public interface IContextualAsyncNotificationHandler<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Handles an asynchronous notification
        /// </summary>
        /// <param name="notification">The notification message</param>
        /// <param name="context">The context</param>
        /// <returns>A task representing handling the notification</returns>
        Task Handle(TNotification notification, IMediatorContext context);
    }
}