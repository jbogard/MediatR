using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines a mediator to encapsulate request/response and publishing interaction patterns
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Send a request to a single handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <returns>Response</returns>
        TResponse Send<TResponse>(IRequest<TResponse> request);

        /// <summary>
        /// Send a request to a single handler without expecting a response
        /// </summary>
        /// <param name="request">Request object</param>
        void Send(IRequest request);

        /// <summary>
        /// Asynchronously send a request to a single handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request);

        /// <summary>
        /// Asynchronously send a request to a single handler without expecting a response
        /// </summary>
        /// <param name="request">Request object</param>
        /// <returns>A task that represents the send operation.</returns>
        Task SendAsync(IAsyncRequest request);

        /// <summary>
        /// Asynchronously send a cancellable request to a single handler
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(ICancellableAsyncRequest<TResponse> request, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously send a cancellable request to a single handler without expecting a response
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the send operation.</returns>
        Task SendAsync(ICancellableAsyncRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        void Publish(INotification notification);

        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(IAsyncNotification notification);

        /// <summary>
        /// Asynchronously send a cancellable notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(ICancellableAsyncNotification notification, CancellationToken cancellationToken);
    }
}