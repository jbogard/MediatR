using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.Tests.ExecutionFlowTests;

internal sealed class RequestResponseAction :
    IRequestResponseExceptionAction<ThrowingExceptionRequest, Response, InvalidOperationException>,
    IRequestResponseExceptionAction<ThrowingExceptionRequest, Response>
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