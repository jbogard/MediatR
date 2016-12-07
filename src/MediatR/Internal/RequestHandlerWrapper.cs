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

    internal class RequestHandlerWrapperImpl<TCommand> : RequestHandlerWrapper
        where TCommand : IRequest
    {
        private readonly IRequestHandler<TCommand> _inner;

        public RequestHandlerWrapperImpl(IRequestHandler<TCommand> inner)
        {
            _inner = inner;
        }

        public override void Handle(IRequest message)
        {
            _inner.Handle((TCommand)message);
        }
    }
    internal class RequestHandlerWrapperImpl<TCommand, TResult> : RequestHandlerWrapper<TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly IRequestHandler<TCommand, TResult> _inner;

        public RequestHandlerWrapperImpl(IRequestHandler<TCommand, TResult> inner)
        {
            _inner = inner;
        }

        public override TResult Handle(IRequest<TResult> message)
        {
            return _inner.Handle((TCommand)message);
        }
    }
}