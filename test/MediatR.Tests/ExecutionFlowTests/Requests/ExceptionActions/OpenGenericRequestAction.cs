using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExecutionFlowTests.Requests.ExceptionActions;

internal sealed class OpenGenericRequestAction<TRequest, TException> : IRequestExceptionAction<TRequest, TException>
    where TRequest : IRequest
    where TException : Exception
{
    public int Calls { get; private set; }

    public Task Execute(TRequest request, TException exception, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.CompletedTask;
    }
}