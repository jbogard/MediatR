using System.IO;
using System.Threading;
using MediatR.Abstraction.Handlers;

namespace MediatR.Examples;

using System.Threading.Tasks;

public class PingHandler : IRequestHandler<Ping, Pong>
{
    private readonly TextWriter _writer;

    public PingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public async ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"--- Handled Ping: {request.Message}");
        return new Pong { Message = request.Message + " Pong" };
    }
}