using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.ExecutionFlowTests.Notification;

internal sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    public int Calls { get; private set; }

    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        if (notification is BaseNotification baseNotification)
        {
            baseNotification.Handlers++;
        }

        Calls++;

        return Task.CompletedTask;
    }
}