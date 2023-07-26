using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.RequestResponse;

internal sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    private static readonly ValueTask<Pong> Response = ValueTask.FromResult(new Pong());

    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken) => Response;
}