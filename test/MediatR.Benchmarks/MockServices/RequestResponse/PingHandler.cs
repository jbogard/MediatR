using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.RequestResponse;

internal sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    private static readonly Task<Pong> Response = Task.FromResult(new Pong());

    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken) => Response;
}