using System.Threading;

namespace MediatR.Subscriptions;

internal abstract class NotificationHandler
{
    public abstract void Handle(INotification notification, CancellationToken cancellationToken);
}