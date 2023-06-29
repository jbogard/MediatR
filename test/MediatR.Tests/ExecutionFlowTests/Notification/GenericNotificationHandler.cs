using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Tests.ExecutionFlowTests;

internal sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    public static int Calls { get; private set; }
    
    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        if (notification is BaseNotification baseNotification)
        {
            baseNotification.Handlers++;
            baseNotification.GenericHandlerType = GetType();
        }

        Calls++;
        
        return Task.CompletedTask;
    }
}