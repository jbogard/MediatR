namespace MediatR.Wrappers;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(object notification, IServiceProvider serviceFactory,
        Func<IEnumerable<NotificationHandlerExecutor>, object, CancellationToken, Task> publish,
        CancellationToken cancellationToken);
}

public class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
{
    public override Task Handle(object notification, IServiceProvider serviceFactory,
        Func<IEnumerable<NotificationHandlerExecutor>, object, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        var handlers = serviceFactory
            .GetServices<INotificationHandler<TNotification>>()
            .Select(static x => new NotificationHandlerExecutor(x, (theNotification, theToken) => x.Handle((TNotification) theNotification, theToken)));

        return publish(handlers, notification, cancellationToken);
    }
}