using System.IO;
using System.Threading;

namespace MediatR.Examples
{
    using System.Threading.Tasks;

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        private readonly TextWriter _writer;

        public PingHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"--- Handled Ping: {request.Message}");
            return new Pong { Message = request.Message + " Pong" };
        }
    }

    public class BroadcastPing1Handler : IRequestHandler<BroadcastPing, Pong>
    {
        private readonly TextWriter _writer;

        public BroadcastPing1Handler(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<Pong> Handle(BroadcastPing request, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"--- Handled Broadcast Ping1: {request.Message}");
            return new Pong { Message = request.Message + " Pong1" };
        }
    }

    public class BroadcastPing2Handler : IRequestHandler<BroadcastPing, Pong>
    {
        private readonly TextWriter _writer;

        public BroadcastPing2Handler(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<Pong> Handle(BroadcastPing request, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"--- Handled Broadcast Ping2: {request.Message}");
            return new Pong { Message = request.Message + " Pong2" };
        }
    }
}