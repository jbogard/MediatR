namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class NotificationProcessor<TNotification>
        where TNotification : INotification
    {
        private readonly IEnumerable<INotificationHandler<TNotification>> _notificationHandlers;

        public NotificationProcessor(IEnumerable<INotificationHandler<TNotification>> notificationHandlers) =>
            _notificationHandlers = notificationHandlers;

        public Task Handle(TNotification notification, CancellationToken cancellationToken, Func<IEnumerable<Task>, Task> publish) =>
            publish(_notificationHandlers.Select(x => x.Handle(notification, cancellationToken)));
    }
}