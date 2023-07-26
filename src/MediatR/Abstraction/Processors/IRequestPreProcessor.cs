using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Abstraction.Processors;

/// <summary>
/// Defined a request pre-processor for a handler
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
public interface IRequestPreProcessor<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Process method executes before calling the Handle method on your handler
    /// </summary>
    /// <param name="request">Incoming request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Process(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Defined a request pre-processor for a handler
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequestPreProcessor<in TRequest,in TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Process method executes before calling the Handle method on your handler
    /// </summary>
    /// <param name="request">Incoming request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Process(TRequest request, CancellationToken cancellationToken);
}