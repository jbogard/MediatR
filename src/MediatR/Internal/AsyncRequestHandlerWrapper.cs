using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class AsyncRequestHandlerWrapper
    {
        public abstract Task Handle(IRequest message, object handler);
    }

    internal abstract class AsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IRequest<TResult> message, object handler);
    }

    internal class AsyncRequestHandlerWrapperImpl<TRequest> : AsyncRequestHandlerWrapper
        where TRequest : IRequest
    {
        public override Task Handle(IRequest message, object handler)
        {
            return ((IAsyncRequestHandler<TRequest>)handler).Handle((TRequest)message);
        }
    }

    internal class AsyncRequestHandlerWrapperImpl<TCommand, TResult> : AsyncRequestHandlerWrapper<TResult>
        where TCommand : IRequest<TResult>
    {
        public override Task<TResult> Handle(IRequest<TResult> message, object handler)
        {
            return ((IAsyncRequestHandler<TCommand, TResult>)handler).Handle((TCommand)message);
        }
    }
}