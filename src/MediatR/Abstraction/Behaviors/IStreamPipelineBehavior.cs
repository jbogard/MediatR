using System.Collections.Generic;
using System.Threading;

namespace MediatR.Abstraction.Behaviors;

/// <summary>
/// Represents an async enumerable continuation for the next task to execute in the pipeline
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
/// <returns>Async Enumerable returning a <typeparamref name="TResponse"/></returns>
public delegate IAsyncEnumerable<TResponse> StreamHandlerNext<in TRequest, out TResponse>(TRequest request, CancellationToken cancellationToken)
    where TRequest : IStreamRequest<TResponse>;

/// <summary>
/// Stream Pipeline behavior to surround the inner handler.
/// Implementations add additional behavior and await the next delegate.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Stream Pipeline handler. Perform any additional behavior and iterate the <paramref name="next"/> delegate as necessary
    /// </summary>
    /// <param name="request">Incoming request</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse"/></returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken);
}