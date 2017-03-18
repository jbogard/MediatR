using System.IO;

namespace MediatR.Examples
{
    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        private readonly TextWriter _writer;

        public PingHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Pong Handle(Ping message)
        {
            _writer.WriteLine($"--- Handled Ping: {message.Message}");
            return new Pong {Message = message.Message + " Pong"};
        }
    }
}