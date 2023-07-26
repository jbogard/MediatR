using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Abstraction.ExceptionHandler;

/// <summary>
/// Defines an exception action for a request with a response.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IRequestResponseExceptionAction<in TRequest, TResponse, in TException>
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    /// <summary>
    /// Executes the Action that should be called when the exception <typeparamref name="TException"/> was thrown while handling <typeparamref name="TRequest"/>.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The action as a Task.</returns>
    Task Execute(TRequest request, TException exception, CancellationToken cancellationToken);
}