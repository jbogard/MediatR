using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.Request;

internal sealed class PingRequestHandler : IRequestHandler<Ping>
{
    public Task Handle(Ping request, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}