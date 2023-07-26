using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.Notification;

internal sealed class PingNotificationHandler : INotificationHandler<PingNotification>
{
    public Task Handle(PingNotification notification, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}