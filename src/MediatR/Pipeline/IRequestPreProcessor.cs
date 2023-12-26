namespace MediatR.Pipeline;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defined a request pre-processor for a handler
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
public interface IRequestPreProcessor<in TRequest> where TRequest : notnull
{
#if NET8_0
    /// <summary>
    /// Controls execution order of any implementations of this request pre-processor.
    /// All implementations are ordered by this field, and order of duplicate
    /// numbers is not guaranteed.  Not overriding this property has the behavior of
    /// all implementations will be executed in the order they are returned by the DI
    /// container.
    /// </summary>
    int Order => 0;
#endif

    /// <summary>
    /// Process method executes before calling the Handle method on your handler
    /// </summary>
    /// <param name="request">Incoming request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An awaitable task</returns>
    Task Process(TRequest request, CancellationToken cancellationToken);
}