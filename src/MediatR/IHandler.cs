namespace MediatR
{
    using System.Threading.Tasks;

    public interface IRequestHandler<in TRequest, out TResponse>
        where TRequest : IRequest<TResponse>
    {
        TResponse Handle(TRequest message);
    }

    public interface IAsyncRequestHandler<in TRequest, TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest message);
    }

    public abstract class RequestHandler<TMessage> : IRequestHandler<TMessage, Unit>
        where TMessage : IRequest
    {
        public Unit Handle(TMessage message)
        {
            HandleCore(message);

            return Unit.Value;
        }

        protected abstract void HandleCore(TMessage message);
    }

    public abstract class AsyncRequestHandler<TMessage> : IAsyncRequestHandler<TMessage, Unit>
        where TMessage : IAsyncRequest
    {
        public async Task<Unit> Handle(TMessage message)
        {
            await HandleCore(message);

            return Unit.Value;
        }

        protected abstract Task HandleCore(TMessage message);
    }

    public interface IPostRequestHandler<in TRequest, in TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task Handle(TRequest request, TResponse response);
    }

    public interface IAsyncPostRequestHandler<in TRequest, in TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        Task Handle(TRequest request, TResponse response);
    }

    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        void Handle(TNotification notification);
    }

    public interface IAsyncNotificationHandler<in TNotification>
        where TNotification : IAsyncNotification
    {
        Task Handle(TNotification notification);
    }
}