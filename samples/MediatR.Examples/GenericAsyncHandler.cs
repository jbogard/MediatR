using System.IO;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    public class GenericAsyncHandler : IAsyncNotificationHandler<INotification>
    {
        private readonly TextWriter _writer;

        public GenericAsyncHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(INotification notification)
        {
            return _writer.WriteLineAsync("Got notified also async.");
        }
    }
}
