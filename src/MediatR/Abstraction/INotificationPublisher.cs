using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Abstraction;

public interface INotificationPublisher
{
    Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification;
}