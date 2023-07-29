using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.ExceptionActions;

internal sealed class RequestAction :
    IRequestExceptionAction<ThrowingExceptionRequest, InvalidOperationException>,
    IRequestExceptionAction<ThrowingExceptionRequest, Exception>
{
    public int InvalidOperationExceptionActionCalls { get; private set; }
    public int GeneralExceptionActionCalls { get; private set; }

    public Task Execute(ThrowingExceptionRequest request, InvalidOperationException exception, CancellationToken cancellationToken)
    {
        if (exception == request.Exception)
        {
            InvalidOperationExceptionActionCalls++;
        }

        return Task.CompletedTask;
    }

    public Task Execute(ThrowingExceptionRequest request, Exception exception, CancellationToken cancellationToken)
    {
        GeneralExceptionActionCalls++;
        return Task.CompletedTask;
    }
}