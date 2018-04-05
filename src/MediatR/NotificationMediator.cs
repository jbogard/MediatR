namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public interface INotificationMediator
    {
        Task Publish(INotification notification, CancellationToken cancellationToken,
            Func<IEnumerable<Task>, Task> publishBehavior);
    }

    public interface INotificationMediator<in TNotification> : INotificationMediator
        where TNotification : INotification
    {
        Task Publish(TNotification notification, CancellationToken cancellationToken,
            Func<IEnumerable<Task>, Task> publishBehavior);
    }

    public class NotificationMediator<TNotification> : INotificationMediator<TNotification>
        where TNotification : INotification
    {
        private readonly IEnumerable<INotificationHandler<TNotification>> _notificationHandlers;

        public NotificationMediator(IEnumerable<INotificationHandler<TNotification>> notificationHandlers) =>
            _notificationHandlers = notificationHandlers;

        public Task Publish(TNotification notification, CancellationToken cancellationToken,
            Func<IEnumerable<Task>, Task> publishBehavior) =>
            publishBehavior(_notificationHandlers.Select(x => x.Handle(notification, cancellationToken)));

        Task INotificationMediator.Publish(INotification notification, CancellationToken cancellationToken,
            Func<IEnumerable<Task>, Task> publishBehavior) =>
            Publish((TNotification) notification, cancellationToken, publishBehavior);
    }
}