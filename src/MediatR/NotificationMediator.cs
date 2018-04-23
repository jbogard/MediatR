namespace MediatR
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Sends notification to multiple handlers
    /// </summary>
    /// <typeparam name="TNotification"></typeparam>
    public interface INotificationMediator<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Sends notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task for operation completion</returns>
        Task Publish(TNotification notification, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    public class NotificationMediator<TNotification> : INotificationMediator<TNotification>
        where TNotification : INotification
    {
        private readonly IEnumerable<INotificationHandler<TNotification>> _notificationHandlers;

        /// <summary>
        /// Constructs the thingy with the notification handlers if any
        /// </summary>
        /// <param name="notificationHandlers"></param>
        public NotificationMediator(IEnumerable<INotificationHandler<TNotification>> notificationHandlers) =>
            _notificationHandlers = notificationHandlers;

        /// <inheritdoc />
        public Task Publish(TNotification notification, CancellationToken cancellationToken) =>
            PublishBehavior(_notificationHandlers.Select(x => x.Handle(notification, cancellationToken)));

        /// <summary>
        /// Override in a derived class to control how the tasks are awaited.
        /// By default the implementation is <see cref="Task.WhenAll(IEnumerable{Task})" />
        /// </summary>
        /// <param name="allHandlers">Enumerable of tasks representing invoking each notification handler</param>
        /// <returns>A task representing invoking all handlers</returns>
        protected virtual Task PublishBehavior(IEnumerable<Task> allHandlers) =>
            Task.WhenAll(allHandlers);
    }
}