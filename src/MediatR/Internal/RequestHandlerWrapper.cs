namespace MediatR.Internal
{
    internal abstract class RequestHandlerWrapper<TResult>
    {
        public abstract TResult Handle(IRequest<TResult> message);
    }

    internal abstract class RequestHandlerWrapper
    {
        public abstract void Handle(IRequest message);
    }

    internal class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
        where TRequest : IRequest
    {
        private readonly IRequestHandler<TRequest> _inner;

        public RequestHandlerWrapperImpl(IRequestHandler<TRequest> inner)
        {
            _inner = inner;
        }

        public override void Handle(IRequest message)
        {
            _inner.Handle((TRequest)message);
        }
    }
    internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _inner;

        public RequestHandlerWrapperImpl(IRequestHandler<TRequest, TResponse> inner)
        {
            _inner = inner;
        }

        public override TResponse Handle(IRequest<TResponse> message)
        {
            return _inner.Handle((TRequest)message);
        }
    }
}