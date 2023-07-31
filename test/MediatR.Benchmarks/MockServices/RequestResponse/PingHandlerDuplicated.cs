using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.RequestResponse;

internal sealed class PingHandlerDuplicated : IRequestHandler<PingPong, Pong>
{
    public ValueTask<Pong> Handle(PingPong request, CancellationToken cancellationToken) => throw new UnreachableException();
}