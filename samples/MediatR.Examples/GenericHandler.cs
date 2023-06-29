using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Examples;

public class GenericHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly TextWriter _writer;

    public GenericHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        return _writer.WriteLineAsync("Got notified.");
    }
}