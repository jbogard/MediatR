using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.RequestResponse;

internal sealed class PingHandler :
    IRequestHandler<PingPong, Pong>,
    IRequestHandler<ThrowingPingPong, Pong>
{
    private static readonly ValueTask<Pong> Response = ValueTask.FromResult(new Pong());

    public ValueTask<Pong> Handle(PingPong request, CancellationToken cancellationToken) => Response;
    public ValueTask<Pong> Handle(ThrowingPingPong request, CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}