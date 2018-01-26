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
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Wrapper class for a handler that asynchronously handles a request and returns a response, ignoring the cancellation token
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public abstract class AsyncRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
            => HandleCore(request);

        /// <summary>
        /// Override in a derived class for the handler logic
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Response</returns>
        protected abstract Task<TResponse> HandleCore(TRequest request);
    }

    /// <summary>
    /// Wrapper class for a handler that asynchronously handles a request and does not return a response, ignoring the cancellation token
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    public abstract class AsyncRequestHandler<TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        public Task Handle(TRequest request, CancellationToken cancellationToken)
            => HandleCore(request);

        /// <summary>
        /// Override in a derived class for the handler logic
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Response</returns>
        protected abstract Task HandleCore(TRequest request);
    }

    /// <summary>
    /// Wrapper class for a handler that synchronously handles a request and returns a response
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
            => Task.FromResult(HandleCore(request));

        /// <summary>
        /// Override in a derived class for the handler logic
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Response</returns>
        protected abstract TResponse HandleCore(TRequest request);
    }

    /// <summary>
    /// Wrapper class for a handler that synchronously handles a request does not return a response
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    public abstract class RequestHandler<TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        public Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            HandleCore(request);
            return Unit.Task;
        }

        protected abstract void HandleCore(TRequest request);
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
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task Handle(TRequest request, CancellationToken cancellationToken);
    }
}