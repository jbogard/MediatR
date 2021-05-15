using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MediatR.AsyncEnumerable
{
    /// <summary>
    /// Defines a handler for a IAsyncEnumerable as return type.
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IAsyncEnumerableRequestHandler<in TRequest, out TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles a request with IAsyncEnumerable as return type.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }


    /// <summary>
    /// Wrapper class for a IAsyncEnumerable request handler that handles a request and returns a response
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public abstract class AsyncEnumerableRequestHandler<TRequest, TResponse> : IAsyncEnumerableRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        async IAsyncEnumerable<TResponse> IAsyncEnumerableRequestHandler<TRequest, TResponse>.Handle(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return await Handle(request);
        }

        /// <summary>
        /// Override in a derived class for the handler logic
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Response</returns>
        protected abstract Task<TResponse> Handle(TRequest request);
    }

}