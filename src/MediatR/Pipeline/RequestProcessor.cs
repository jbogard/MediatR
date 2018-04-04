namespace MediatR.Pipeline
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class UntypedRequestProcessor<TResponse>
    {
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken);
    }

    public class RequestProcessor<TRequest, TResponse> : UntypedRequestProcessor<TResponse>
            where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _requestHandler;
        private readonly IEnumerable<IPipelineBehavior<TRequest, TResponse>> _pipelineBehaviors;

        public RequestProcessor(IRequestHandler<TRequest, TResponse> requestHandler,
            IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelineBehaviors)
        {
            _requestHandler = requestHandler;
            _pipelineBehaviors = pipelineBehaviors;
        }

        public override Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken) =>
            _pipelineBehaviors.Reverse().Aggregate(new RequestHandlerDelegate<TResponse>(
                    () => _requestHandler.Handle((TRequest)request, cancellationToken)),
                    (next, behavior) => () => behavior.Handle((TRequest)request, cancellationToken, next))
                .Invoke();
    }

    // todo: It would be possible to drop this specialization if only IRequestHandler<TRequest> was implementing IRequestHandler<TRequest, Unit
    // or if major containers (StructureMap) supported matching registered `VoidRequestHandlerAdapter<TRequest>` with `IRequestHandler<TRequest, Unit>`
    public class RequestProcessor<TRequest> : RequestProcessor<TRequest, Unit>
        where TRequest : IRequest
    {
        public RequestProcessor(IRequestHandler<TRequest> requestHandler, IEnumerable<IPipelineBehavior<TRequest, Unit>> pipelineBehaviors)
            : base(new HandlerAdapter(requestHandler), pipelineBehaviors) { }

        private sealed class HandlerAdapter : IRequestHandler<TRequest, Unit>
        {
            private readonly IRequestHandler<TRequest> _adapted;
            public HandlerAdapter(IRequestHandler<TRequest> adapted) => _adapted = adapted;
            public async Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
            {
                await _adapted.Handle(request, cancellationToken);
                return Unit.Value;
            }
        }
    }
}
