using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Abstraction.ExceptionHandler;

/// <summary>
/// Defines an exception action for a request
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IRequestExceptionAction<in TRequest, in TException>
    where TRequest : IRequest
    where TException : Exception
{
    /// <summary>
    /// Called when the request handler throws an exception
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="exception">The thrown exception</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Execute(TRequest request, TException exception, CancellationToken cancellationToken);
}

/// <summary>
/// Defines the base exception action for a request.
///     You do not need to register this interface explicitly
///     with a container as it inherits from the base
///     <see cref="IRequestExceptionAction{TRequest, TException}" /> interface.
/// </summary>
/// <typeparam name="TRequest">The type of failed request</typeparam>
public interface IRequestExceptionAction<in TRequest> : IRequestExceptionAction<TRequest, Exception>
    where TRequest : IRequest
{
}