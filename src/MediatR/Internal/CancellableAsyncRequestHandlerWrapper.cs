using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class CancellableAsyncRequestHandlerWrapper<TResult>
    {
        public abstract Task<TResult> Handle(IRequest<TResult> message, CancellationToken cancellationToken, object handler);
    }

    internal class CancellableAsyncRequestHandlerWrapperImpl<TRequest, TResponse> : CancellableAsyncRequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Handle(IRequest<TResponse> message, CancellationToken cancellationToken, object handler)
        {
            return ((ICancellableAsyncRequestHandler<TRequest, TResponse>)handler).Handle((TRequest)message, cancellationToken);
        }
    }
}