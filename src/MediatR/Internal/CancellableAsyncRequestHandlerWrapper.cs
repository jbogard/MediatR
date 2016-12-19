using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class CancellableAsyncRequestHandlerWrapper
    {
        public abstract Task Handle(IRequest message, CancellationToken cancellationToken);
    }

    internal abstract class CancellableAsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IRequest<TResult> message, CancellationToken cancellationToken);
    }

    internal class CancellableAsyncRequestHandlerWrapperImpl<TRequest> : CancellableAsyncRequestHandlerWrapper
        where TRequest : IRequest
    {
        private readonly ICancellableAsyncRequestHandler<TRequest> _inner;

        public CancellableAsyncRequestHandlerWrapperImpl(ICancellableAsyncRequestHandler<TRequest> inner)
        {
            _inner = inner;
        }

        public override Task Handle(IRequest message, CancellationToken cancellationToken)
        {
            return _inner.Handle((TRequest)message, cancellationToken);
        }
    }

    internal class CancellableAsyncRequestHandlerWrapperImpl<TRequest, TResponse> : CancellableAsyncRequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICancellableAsyncRequestHandler<TRequest, TResponse> _inner;

        public CancellableAsyncRequestHandlerWrapperImpl(ICancellableAsyncRequestHandler<TRequest, TResponse> inner)
        {
            _inner = inner;
        }

        public override Task<TResponse> Handle(IRequest<TResponse> message, CancellationToken cancellationToken)
        {
            return _inner.Handle((TRequest)message, cancellationToken);
        }
    }
}