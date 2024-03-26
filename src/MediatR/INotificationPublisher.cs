using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using MediatR.Contracts;

namespace MediatR;

public interface INotificationPublisher
{
    Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
        CancellationToken cancellationToken);
}