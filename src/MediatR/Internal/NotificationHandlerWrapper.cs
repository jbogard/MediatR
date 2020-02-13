namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Reflection;
    using NotificationHandlersOrder;

    internal abstract class NotificationHandlerWrapper
    {
        public abstract Task Handle(INotification notification, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish);
    }

    internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        public override Task Handle(INotification notification, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
        {
            var allHandlers = serviceFactory
                .GetInstances<INotificationHandler<TNotification>>()
                .ToArray();

            IEnumerable<Func<INotification, CancellationToken, Task>> handlersToPublish;

            if (allHandlers.All(x => x.GetType().GetCustomAttribute<NotificationOrderAttribute>() != null))
            {
                var groupedHandlers = allHandlers
                    .GroupBy(x => x.GetType().GetCustomAttribute<NotificationOrderAttribute>().Value)
                    .OrderBy(x => x.Key);

                foreach (var handlersGroup in groupedHandlers)
                {
                    handlersToPublish = handlersGroup.Select(x =>
                        new Func<INotification, CancellationToken, Task>((theNotification, theToken) =>
                            x.Handle((TNotification) theNotification, theToken)));

                    publish(handlersToPublish, notification, cancellationToken);
                }
            }

            handlersToPublish = allHandlers.Select(x =>
                new Func<INotification, CancellationToken, Task>((theNotification, theToken) =>
                    x.Handle((TNotification) theNotification, theToken)));

            return publish(handlersToPublish, notification, cancellationToken);
        }
    }
}