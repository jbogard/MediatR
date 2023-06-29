using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Tests.ExecutionFlowTests;

internal sealed class BaseNotificationHandler : INotificationHandler<BaseNotification>
{
    public int Calls { get; private set; }

    public Task Handle(BaseNotification notification, CancellationToken cancellationToken)
    {
        notification.Handlers++;
        
        Calls++;
        
        return Task.CompletedTask;
    }
}