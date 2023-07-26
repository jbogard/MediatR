namespace MediatR.ExecutionFlowTests.Notification;

internal abstract class BaseNotification : INotification
{
    public int Handlers { get; set; }
}