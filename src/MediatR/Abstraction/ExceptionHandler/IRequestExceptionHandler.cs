using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.ExceptionHandling;

namespace MediatR.Abstraction.ExceptionHandler;

/// <summary>
/// Defines an exception handler for a request and response
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IRequestExceptionHandler<in TRequest, in TException>
    where TRequest : IRequest
    where TException : Exception
{
    /// <summary>
    /// Called when the request handler throws an exception
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="exception">The thrown exception</param>
    /// <param name="state">The current state of the exception.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Handle(TRequest request, TException exception, RequestExceptionHandlerState state, CancellationToken cancellationToken);
}

/// <summary>
/// Defines the base exception handler for a request and response
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
public interface IRequestExceptionHandler<in TRequest> : IRequestExceptionHandler<TRequest, Exception>
    where TRequest : IRequest
{
}