using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Abstraction.Behaviors;

/// <summary>
/// Represents an async continuation for the next task to execute in the pipeline
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
public delegate Task RequestHandlerDelegate<in TRequest>(TRequest request, CancellationToken cancellationToken)
    where TRequest : IRequest;

/// <summary>
/// Pipeline behavior to surround the inner handler.
/// Implementations add additional behavior and await the next delegate.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
public interface IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Pipeline handler. Perform any additional behavior and await the <paramref name="next"/> delegate as necessary
    /// </summary>
    /// <param name="request">Incoming request</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken);
}