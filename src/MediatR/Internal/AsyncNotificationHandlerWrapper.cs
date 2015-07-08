using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class AsyncNotificationHandlerWrapper
    {
        public abstract Task Handle(IAsyncNotification message);
    }

    internal class AsyncNotificationHandlerWrapper<TNotification> : AsyncNotificationHandlerWrapper
        where TNotification : IAsyncNotification
    {
        private readonly IAsyncNotificationHandler<TNotification> _inner;

        public AsyncNotificationHandlerWrapper(IAsyncNotificationHandler<TNotification> inner)
        {
            _inner = inner;
        }

        public override Task Handle(IAsyncNotification message)
        {
            return _inner.Handle((TNotification)message);
        }
    }
}