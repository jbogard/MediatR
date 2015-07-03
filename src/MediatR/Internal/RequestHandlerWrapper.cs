namespace MediatR.Internal
{
    internal abstract class RequestHandlerWrapper<TResult>
    {
        public abstract TResult Handle(IRequest<TResult> message);
    }

    internal class RequestHandlerWrapper<TCommand, TResult> : RequestHandlerWrapper<TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly IRequestHandler<TCommand, TResult> _inner;

        public RequestHandlerWrapper(IRequestHandler<TCommand, TResult> inner)
        {
            _inner = inner;
        }

        public override TResult Handle(IRequest<TResult> message)
        {
            return _inner.Handle((TCommand)message);
        }
    }
}