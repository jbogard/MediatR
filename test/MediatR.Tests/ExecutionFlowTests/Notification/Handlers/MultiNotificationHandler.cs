using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.ExecutionFlowTests.Notification;

internal sealed class MultiNotificationHandler : INotificationHandler<BaseNotification>, INotificationHandler<Notification>
{
    public int Calls { get; private set; }

    public Task Handle(BaseNotification notification, CancellationToken cancellationToken)
    {
        notification.Handlers++;

        lock (this)
        {
            Calls++;
        }

        return Task.CompletedTask;
    }

    public Task Handle(Notification notification, CancellationToken cancellationToken)
    {
        notification.Message = "Was handled by multi handler";
        notification.Handlers++;

        lock (this)
        {
            Calls++;
        }

        return Task.CompletedTask;
    }
}