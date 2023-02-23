namespace MediatR.Wrappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Internal;
using Microsoft.Extensions.DependencyInjection;

public abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(INotification notification, IServiceProvider serviceFactory,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken);
}

public class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override Task Handle(INotification notification, IServiceProvider serviceFactory,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        var handlers = typeof(TNotification)
            // For each of base types of TNotification, including TNotification itself
            .GetBaseTypes(static (t, _) => t.GetInterfaces().Any(static i => i == typeof(INotification)), null)
            .Append(typeof(TNotification))

            // Resolve all INotificationHandler<...> for the type from service provider
            .Select(static t => typeof(INotificationHandler<>).MakeGenericType(t))
            .SelectMany(serviceFactory.GetServices)
            .Distinct(new ObjectTypeEqualityComparer())

            // And create executors
            .Cast<INotificationHandler<TNotification>>()
            .Select(static x => new NotificationHandlerExecutor(x, (theNotification, theToken) => x.Handle((TNotification) theNotification, theToken)));

        return publish(handlers, notification, cancellationToken);
    }
}