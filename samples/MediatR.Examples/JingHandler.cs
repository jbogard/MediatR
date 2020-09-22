using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    public class JingHandler : AsyncRequestHandler<Jing>
    {
        private readonly TextWriter _writer;

        public JingHandler(TextWriter writer)
        {
            _writer = writer;
        }

        protected override Task HandleAsync(Jing request, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync($"--- Handled Jing: {request.Message}, no Jong");
        }
    }
}
