using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    using System.IO;

    public class GenericHandler : INotificationHandler<INotification>
    {
        private readonly TextWriter _writer;

        public GenericHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(INotification notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("Got notified.");
        }
    }
}