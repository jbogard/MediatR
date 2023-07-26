using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.ExceptionHandling.RequestResponse.Subscription;

namespace MediatR.Abstraction.ExceptionHandler;

/// <summary>
/// Defines an exception handler for the exception <typeparamref name="TException"/> while handling the request <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response for the request.</typeparam>
/// <typeparam name="TException">The exception that should be handled.</typeparam>
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