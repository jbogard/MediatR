using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines a handler for a request
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        Task<TResponse> Handle(TRequest message, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines a handler for a request without a return value
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    public interface IRequestHandler<in TRequest>
        where TRequest : IRequest
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task Handle(TRequest message, CancellationToken cancellationToken);
    }
}