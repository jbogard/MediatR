namespace MediatR
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRequestMediator<TResponse>
    {
        Task<TResponse> Send(IRequest<TResponse> request, CancellationToken cancellationToken);
    }

    public interface IRequestMediator<in TRequest, TResponse> : IRequestMediator<TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Send(TRequest request, CancellationToken cancellationToken);
    }

    public class RequestMediator<TRequest, TResponse> : IRequestMediator<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _requestHandler;
        private readonly IEnumerable<IPipelineBehavior<TRequest, TResponse>> _pipelineBehaviors;

        public RequestMediator(IRequestHandler<TRequest, TResponse> requestHandler,
            IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelineBehaviors)
        {
            _requestHandler = requestHandler;
            _pipelineBehaviors = pipelineBehaviors;
        }

        public Task<TResponse> Send(TRequest request, CancellationToken cancellationToken) =>
            _pipelineBehaviors.Reverse().Aggregate(new RequestHandlerDelegate<TResponse>(
                () => _requestHandler.Handle(request, cancellationToken)),
                (next, behavior) => () => behavior.Handle(request, cancellationToken, next))
                .Invoke();

        Task<TResponse> IRequestMediator<TResponse>.Send(IRequest<TResponse> request, CancellationToken cancellationToken) =>
            Send((TRequest)request, cancellationToken);
    }
}
