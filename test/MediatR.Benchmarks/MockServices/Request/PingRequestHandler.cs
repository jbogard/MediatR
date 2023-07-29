using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.Request;

internal sealed class PingRequestHandler :
    IRequestHandler<Ping>,
    IRequestHandler<ThrowingPing>
{
    public ValueTask Handle(Ping request, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask Handle(ThrowingPing request, CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}