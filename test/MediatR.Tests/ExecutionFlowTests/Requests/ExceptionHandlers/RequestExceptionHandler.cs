using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExecutionFlowTests.Requests.ExceptionHandlers;

internal sealed class RequestExceptionHandler :
    IRequestExceptionHandler<RequestMessages.ThrowingExceptionRequest, InvalidOperationException>,
    IRequestExceptionHandler<RequestMessages.ThrowingExceptionRequest, Exception>
{
    public int InvalidOperationExceptionHandlerCalls { get; private set; }
    public int GeneralExceptionHandlerCalls { get; private set; }

    public Task Handle(RequestMessages.ThrowingExceptionRequest request, InvalidOperationException exception, RequestExceptionHandlerState state, CancellationToken cancellationToken)
    {
        if (request.Exception == exception)
        {
            state.SetHandled();
        }

        InvalidOperationExceptionHandlerCalls++;

        return Task.CompletedTask;
    }

    public Task Handle(RequestMessages.ThrowingExceptionRequest request, Exception exception, RequestExceptionHandlerState state, CancellationToken cancellationToken)
    {
        state.SetHandled();

        GeneralExceptionHandlerCalls++;
        return Task.CompletedTask;
    }
}