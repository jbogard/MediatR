using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.Benchmarks.MockServices.Request;

internal sealed class ExceptionHandler : IRequestExceptionHandler<ThrowingPing, NotSupportedException>
{
    public Task Handle(ThrowingPing request, NotSupportedException exception, RequestExceptionHandlerState state, CancellationToken cancellationToken)
    {
        state.SetHandled();
        return Task.CompletedTask;
    }
}