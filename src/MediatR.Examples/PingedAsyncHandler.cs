namespace MediatR.Examples
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class PingedAsyncHandler : IAsyncNotificationHandler<PingedAsync>
    {
        private readonly TextWriter _writer;

        public PingedAsyncHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task Handle(PingedAsync notification, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync("Got pinged async.");
        }
    }

    public class PingedAlsoAsyncHandler : IAsyncNotificationHandler<PingedAsync>
    {
        private readonly TextWriter _writer;

        public PingedAlsoAsyncHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task Handle(PingedAsync notification, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync("Got pinged also async.");
        }
    }
}