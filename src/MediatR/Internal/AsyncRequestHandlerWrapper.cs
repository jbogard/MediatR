using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class AsyncRequestHandlerWrapper
    {
        public abstract Task Handle(IAsyncRequest message);
    }

    internal abstract class AsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IAsyncRequest<TResult> message);
    }

    internal class AsyncRequestHandlerWrapperImpl<TCommand> : AsyncRequestHandlerWrapper
        where TCommand : IAsyncRequest
    {
        private readonly IAsyncRequestHandler<TCommand> _inner;

        public AsyncRequestHandlerWrapperImpl(IAsyncRequestHandler<TCommand> inner)
        {
            _inner = inner;
        }

        public override Task Handle(IAsyncRequest message)
        {
            return _inner.Handle((TCommand)message);
        }
    }

    internal class AsyncRequestHandlerWrapperImpl<TCommand, TResult> : AsyncRequestHandlerWrapper<TResult>
        where TCommand : IAsyncRequest<TResult>
    {
        private readonly IAsyncRequestHandler<TCommand, TResult> _inner;

        public AsyncRequestHandlerWrapperImpl(IAsyncRequestHandler<TCommand, TResult> inner)
        {
            _inner = inner;
        }

        public override Task<TResult> Handle(IAsyncRequest<TResult> message)
        {
            return _inner.Handle((TCommand)message);
        }
    }
}