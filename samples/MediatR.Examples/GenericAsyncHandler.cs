using System.IO;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    public class GenericAsyncHandler : IAsyncNotificationHandler<IAsyncNotification>
    {
        private readonly TextWriter _writer;

        public GenericAsyncHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task Handle(IAsyncNotification notification)
        {
            await _writer.WriteLineAsync("Got notified also async.");
        }
    }
}
