using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.ExceptionHandling;

namespace MediatR.Abstraction.ExceptionHandler;

public interface IRequestResponseExceptionHandler<in TRequest, TResponse,in TException>
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    /// <summary>
    /// Called when the request handler throws an exception
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="exception">The thrown exception</param>
    /// <param name="state">The current state of handling the exception</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Handle(TRequest request, TException exception, RequestResponseExceptionHandlerState<TResponse> state, CancellationToken cancellationToken);
}

public interface IRequestResponseExceptionHandler<in TRequest, TResponse> : IRequestResponseExceptionHandler<TRequest, TResponse, Exception>
    where TRequest : IRequest<TResponse>
{
}