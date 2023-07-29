using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.Benchmarks.MockServices.RequestResponse;

internal sealed class ExceptionHandler : IRequestResponseExceptionHandler<ThrowingPingPong, Pong, NotSupportedException>
{
    private static readonly Pong response = new();

    public Task Handle(ThrowingPingPong request, NotSupportedException exception, RequestResponseExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
    {
        state.SetHandled(response);
        return Task.CompletedTask;
    }
}