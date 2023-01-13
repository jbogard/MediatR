using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Wrappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Internal;

public abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(INotification notification, IServiceProvider serviceFactory,
        Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken);
}

public class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override Task Handle(INotification notification, IServiceProvider serviceFactory,
        Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        var notificationType = notification.GetType();

        var typesToScan = notificationType.GetBaseTypes(
            static (t, _) => t.GetInterfaces().Any(static i => i == typeof(INotification)),
            null);
        typesToScan = typesToScan.Append(notificationType);

        var handlers = typesToScan
            .Select(static t => typeof(INotificationHandler<>).MakeGenericType(t))
            .SelectMany(handlerType => serviceFactory.GetServices(handlerType), (HandlerType, Handler) => new { HandlerType, Handler })
            .Select(static x => new Func<INotification, CancellationToken, Task>((theNotification, theToken) =>
                {
                    var mi = x.HandlerType.GetMethod(nameof(INotificationHandler<TNotification>.Handle));
                    return (Task) mi.Invoke(x.Handler, new object[] { (TNotification) theNotification, theToken });
                }));

        return publish(handlers, notification, cancellationToken);
    }
}