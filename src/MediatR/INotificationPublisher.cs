using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR;

public interface INotificationPublisher
{
    Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, object notification,
        CancellationToken cancellationToken);
}