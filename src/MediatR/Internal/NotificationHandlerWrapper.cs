namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class NotificationHandlerWrapper
    {
        public abstract Task HandleAsync(INotification notification, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish);
    }

    internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        public override Task HandleAsync(INotification notification, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
        {
            var handlers = serviceFactory
                .GetInstances<INotificationHandler<TNotification>>()
                .Select(x => new Func<INotification, CancellationToken, Task>((theNotification, theToken) => x.HandleAsync((TNotification) theNotification, theToken)));

            return publish(handlers, notification, cancellationToken);
        }
    }
}