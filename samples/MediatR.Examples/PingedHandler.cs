using System.Threading;

namespace MediatR.Examples
{
    using System.IO;
    using System.Threading.Tasks;

    public class PingedHandler : INotificationHandler<Pinged>
    {
        private readonly TextWriter _writer;

        public PingedHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task HandleAsync(Pinged notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("Got pinged async.");
        }
    }

    public class PongedHandler : INotificationHandler<Ponged>
    {
        private readonly TextWriter _writer;

        public PongedHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task HandleAsync(Ponged notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("Got ponged async.");
        }
    }

    public class ConstrainedPingedHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : Pinged
    {
        private readonly TextWriter _writer;

        public ConstrainedPingedHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task HandleAsync(TNotification notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("Got pinged constrained async.");
        }
    }

    public class PingedAlsoHandler : INotificationHandler<Pinged>
    {
        private readonly TextWriter _writer;

        public PingedAlsoHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task HandleAsync(Pinged notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("Got pinged also async.");
        }
    }
}