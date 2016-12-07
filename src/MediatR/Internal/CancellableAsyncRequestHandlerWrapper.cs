using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class CancellableAsyncRequestHandlerWrapper
    {
        public abstract Task Handle(ICancellableAsyncRequest message, CancellationToken cancellationToken);
    }

    internal abstract class CancellableAsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(ICancellableAsyncRequest<TResult> message, CancellationToken cancellationToken);
    }

    internal class CancellableAsyncRequestHandlerWrapperImpl<TCommand> : CancellableAsyncRequestHandlerWrapper
        where TCommand : ICancellableAsyncRequest
    {
        private readonly ICancellableAsyncRequestHandler<TCommand> _inner;

        public CancellableAsyncRequestHandlerWrapperImpl(ICancellableAsyncRequestHandler<TCommand> inner)
        {
            _inner = inner;
        }

        public override Task Handle(ICancellableAsyncRequest message, CancellationToken cancellationToken)
        {
            return _inner.Handle((TCommand)message, cancellationToken);
        }
    }

    internal class CancellableAsyncRequestHandlerWrapperImpl<TCommand, TResult> : CancellableAsyncRequestHandlerWrapper<TResult>
        where TCommand : ICancellableAsyncRequest<TResult>
    {
        private readonly ICancellableAsyncRequestHandler<TCommand, TResult> _inner;

        public CancellableAsyncRequestHandlerWrapperImpl(ICancellableAsyncRequestHandler<TCommand, TResult> inner)
        {
            _inner = inner;
        }

        public override Task<TResult> Handle(ICancellableAsyncRequest<TResult> message, CancellationToken cancellationToken)
        {
            return _inner.Handle((TCommand)message, cancellationToken);
        }
    }
}