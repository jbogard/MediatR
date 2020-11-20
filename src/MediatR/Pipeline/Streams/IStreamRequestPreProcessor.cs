namespace MediatR.Pipeline.Streams
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defined a stream request pre-processor for a handler
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    public interface IStreamRequestPreProcessor<in TRequest> where TRequest : notnull
    {
        /// <summary>
        /// Process method executes before calling the Handle method on your handler
        /// </summary>
        /// <param name="request">Incoming request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An awaitable task</returns>
        Task Process(TRequest request, CancellationToken cancellationToken);
    }
}