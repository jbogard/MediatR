using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines an asynchronous handler for a request
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IAsyncRequestHandler<in TRequest, TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        /// <summary>
        /// Handles an asynchronous request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <returns>A task representing the response from the request</returns>
        Task<TResponse> Handle(TRequest message);
    }

    /// <summary>
    /// Defines an asynchronous handler for a request without a response
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    public interface IAsyncRequestHandler<in TRequest>
        where TRequest : IAsyncRequest
    {
        /// <summary>
        /// Handles an asynchronous request
        /// </summary>
        /// <param name="message">The request message</param>
        Task Handle(TRequest message);
    }

}