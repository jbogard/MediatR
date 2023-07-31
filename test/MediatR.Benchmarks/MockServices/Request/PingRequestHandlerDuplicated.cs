using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.Request;

internal sealed class PingRequestHandlerDuplicated : IRequestHandler<Ping>
{
    public ValueTask Handle(Ping request, CancellationToken cancellationToken) => throw new UnreachableException();
}