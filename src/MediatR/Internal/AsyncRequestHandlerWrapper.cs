using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class AsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IAsyncRequest<TResult> message, CancellationToken cancellationToken = default(CancellationToken));
    }

    internal class AsyncRequestHandlerWrapper<TCommand, TResult> : AsyncRequestHandlerWrapper<TResult>
        where TCommand : IAsyncRequest<TResult>
    {
        private readonly IAsyncRequestHandler<TCommand, TResult> _inner;

        public AsyncRequestHandlerWrapper(IAsyncRequestHandler<TCommand, TResult> inner)
        {
            _inner = inner;
        }

        public override Task<TResult> Handle(IAsyncRequest<TResult> message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _inner.Handle((TCommand)message, cancellationToken);
        }
    }
}