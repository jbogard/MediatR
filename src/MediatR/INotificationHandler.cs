namespace MediatR
{
    /// <summary>
    /// Defines a handler for a notification
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled</typeparam>
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Handles a notification
        /// </summary>
        /// <param name="notification">The notification message</param>
        void Handle(TNotification notification);
    }
}