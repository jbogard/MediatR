namespace MediatR.Internal
{
    internal abstract class RequestHandlerWrapper<TResult>
    {
        public abstract TResult Handle(IRequest<TResult> message, object handler);
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