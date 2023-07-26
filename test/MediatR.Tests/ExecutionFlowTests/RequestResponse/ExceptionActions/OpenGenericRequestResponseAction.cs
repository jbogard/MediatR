using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExecutionFlowTests.RequestResponse;

internal sealed class OpenGenericRequestResponseAction<TRequest, TResponse, TException> : IRequestResponseExceptionAction<TRequest, TResponse, TException>
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    public int Calls { get; private set; }

    public Task Execute(TRequest request, TException exception, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.CompletedTask;
    }
}