namespace MediatR.Pipeline;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines a request post-processor for a request
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequestPostProcessor<in TRequest, in TResponse> where TRequest : notnull
{
#if NET8_0
    /// <summary>
    /// Controls execution order of any implementations of this request post-processor.
    /// All implementations are ordered by this field, and order of duplicate
    /// numbers is not guaranteed.  Not overriding this property has the behavior of
    /// all implementations will be executed in the order they are returned by the DI
    /// container.
    /// </summary>
    int Order => 0;
#endif

    /// <summary>
    /// Process method executes after the Handle method on your handler
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="response">Response instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
}