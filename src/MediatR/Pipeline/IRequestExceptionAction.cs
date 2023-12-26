namespace MediatR.Pipeline;

using System;
using System.Threading;
using System.Threading.Tasks;
    
/// <summary>
/// Defines an exception action for a request
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IRequestExceptionAction<in TRequest, in TException>
    where TRequest : notnull
    where TException : Exception
{
#if NET8_0
    /// <summary>
    /// Controls execution order of any implementations of this request exception-action.
    /// All implementations are ordered by this field, and order of duplicate
    /// numbers is not guaranteed.  Not overriding this property has the behavior of
    /// all implementations will be executed in the order they are returned by the DI
    /// container.
    /// </summary>
    int Order => 0;
#endif

    /// <summary>
    /// Called when the request handler throws an exception
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="exception">The thrown exception</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Execute(TRequest request, TException exception, CancellationToken cancellationToken);
}
