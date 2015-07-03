using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class CancellableAsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(ICancellableAsyncRequest<TResult> message, CancellationToken cancellationToken);
    }

    internal class CancellableAsyncRequestHandlerWrapper<TCommand, TResult> : CancellableAsyncRequestHandlerWrapper<TResult>
        where TCommand : ICancellableAsyncRequest<TResult>
    {
        private readonly ICancellableAsyncRequestHandler<TCommand, TResult> _inner;

        public CancellableAsyncRequestHandlerWrapper(ICancellableAsyncRequestHandler<TCommand, TResult> inner)
        {
            _inner = inner;
        }

        public override Task<TResult> Handle(ICancellableAsyncRequest<TResult> message, CancellationToken cancellationToken)
        {
            return _inner.Handle((TCommand)message, cancellationToken);
        }
    }
}