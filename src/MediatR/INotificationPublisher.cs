using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace MediatR;

public interface INotificationPublisher
{
    Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
        CancellationToken cancellationToken);
}