namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class NotificationHandler
    {
        public abstract Task Handle(INotification notification, CancellationToken cancellationToken, MultiInstanceFactory multiInstanceFactory, Func<IEnumerable<Task>, Task> publish);
    }

    internal class NotificationHandlerImpl<TNotification> : NotificationHandler
        where TNotification : INotification
    {
        public override Task Handle(INotification notification, CancellationToken cancellationToken, MultiInstanceFactory multiInstanceFactory, Func<IEnumerable<Task>, Task> publish)
        {
            var handlers = GetHandlers((TNotification)notification, cancellationToken, multiInstanceFactory);
            return publish(handlers);
        }

        private static IEnumerable<THandler> GetHandlers<THandler>(MultiInstanceFactory factory)
        {
            return factory(typeof(THandler)).Cast<THandler>();
        }

        private IEnumerable<Task> GetHandlers(TNotification notification, CancellationToken cancellationToken, MultiInstanceFactory factory)
        {
            var notificationHandlers = GetHandlers<INotificationHandler<TNotification>>(factory)
                .Select(x =>
                    {
                        x.Handle(notification);
                        return Unit.Task;
                    });

            var asyncNotificationHandlers = GetHandlers<IAsyncNotificationHandler<TNotification>>(factory)
                .Select(x => x.Handle(notification));

            var cancellableAsyncNotificationHandlers = GetHandlers<ICancellableAsyncNotificationHandler<TNotification>>(factory)
                .Select(x => x.Handle(notification, cancellationToken));

            var allHandlers = notificationHandlers
                .Concat(asyncNotificationHandlers)
                .Concat(cancellableAsyncNotificationHandlers);

            return allHandlers;
        }
    }
}