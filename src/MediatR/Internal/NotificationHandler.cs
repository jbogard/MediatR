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
            var handlers = multiInstanceFactory(typeof(INotificationHandler<TNotification>))
                .Cast<INotificationHandler<TNotification>>()
                .Select(x => x.Handle((TNotification)notification, cancellationToken));

            return publish(handlers);
        }
    }
}