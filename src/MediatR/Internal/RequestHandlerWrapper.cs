namespace MediatR.Internal
{
    internal abstract class RequestHandlerWrapper<TResult>
    {
        public abstract TResult Handle(IRequest<TResult> message, object handler);
    }

    internal abstract class RequestHandlerWrapper
    {
        public abstract void Handle(IRequest message, object handler);
    }

    internal class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
        where TRequest : IRequest
    {
        public override void Handle(IRequest message, object handler)
        {
            ((IRequestHandler<TRequest>)handler).Handle((TRequest)message);
        }
    }

    internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override TResponse Handle(IRequest<TResponse> message, object handler)
        {
            return ((IRequestHandler<TRequest, TResponse>)handler).Handle((TRequest)message);
        }
    }
}