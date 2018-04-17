namespace MediatR
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Sends request to the pipeline consisting of request handler and pipeline behaviors.
    ///     The type of request is specidied by its runtime type!
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IRequestMediator<TResponse>
    {
        /// <summary>
        ///     Sends request to the pipeline consisting of request handler and pipeline behaviors if any.
        /// </summary>
        /// <param name="request">Request object, its actual type is used for determining the handler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task with promised response</returns>
        Task<TResponse> Send(IRequest<TResponse> request, CancellationToken cancellationToken);
    }

    /// <summary>
    ///     Sends request to the pipeline consisting of request handler and pipeline behaviors if any.
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IRequestMediator<in TRequest, TResponse> : IRequestMediator<TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        ///     Sends request to the pipeline consisting of request handler and pipeline behaviors if any.
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task with promised response</returns>
        Task<TResponse> Send(TRequest request, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    public class RequestMediator<TRequest, TResponse> : IRequestMediator<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _requestHandler;
        private readonly IEnumerable<IPipelineBehavior<TRequest, TResponse>> _pipelineBehaviors;

        /// <summary>
        /// Constructs the thingy.
        /// </summary>
        /// <param name="requestHandler">Request handler</param>
        /// <param name="pipelineBehaviors">Pipeline behaviors</param>
        public RequestMediator(IRequestHandler<TRequest, TResponse> requestHandler,
            IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelineBehaviors)
        {
            _requestHandler = requestHandler;
            _pipelineBehaviors = pipelineBehaviors;
        }

        /// <inheritdoc />
        public Task<TResponse> Send(TRequest request, CancellationToken cancellationToken) =>
            _pipelineBehaviors.Reverse().Aggregate(new RequestHandlerDelegate<TResponse>(
                () => _requestHandler.Handle(request, cancellationToken)),
                (next, behavior) => () => behavior.Handle(request, cancellationToken, next))
                .Invoke();

        Task<TResponse> IRequestMediator<TResponse>.Send(IRequest<TResponse> request, CancellationToken cancellationToken) =>
            Send((TRequest)request, cancellationToken);
    }
}
