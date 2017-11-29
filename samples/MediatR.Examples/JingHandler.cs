using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    public class JingHandler : IRequestHandler<Jing>
    {
        private readonly TextWriter _writer;

        public JingHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(Jing message, CancellationToken token)
        {
            return _writer.WriteLineAsync($"--- Handled Jing: {message.Message}, no Jong");
        }
    }
}
