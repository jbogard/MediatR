using System.IO;

namespace MediatR.Examples
{
    using System.Threading.Tasks;

    public class PingAsyncHandler : IAsyncRequestHandler<PingAsync, Pong>
    {
        private readonly TextWriter _writer;

        public PingAsyncHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task<Pong> Handle(PingAsync message)
        {
            _writer.WriteLine($"--- Handled Ping: {message.Message}");
            return Task.FromResult(new Pong { Message = message.Message + " Pong" });
        }
    }
}