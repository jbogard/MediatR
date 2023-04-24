using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples;

public class GenericHandler : INotificationHandler<object>
{
    private readonly TextWriter _writer;

    public GenericHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public Task Handle(object notification, CancellationToken cancellationToken)
    {
        return _writer.WriteLineAsync("Got notified.");
    }
}