using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Examples;

public class JingHandler : IRequestHandler<Jing>
{
    private readonly TextWriter _writer;

    public JingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public async Task Handle(Jing request, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"--- Handled Jing: {request.Message}, no Jong");
    }
}