using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Internal
{
    internal abstract class CancellableAsyncNotificationHandlerWrapper
    {
        public abstract Task Handle(INotification message, CancellationToken cancellationToken);
    }

    internal class CancellableAsyncNotificationHandlerWrapper<TNotification> : CancellableAsyncNotificationHandlerWrapper
        where TNotification : INotification
    {
        private readonly ICancellableAsyncNotificationHandler<TNotification> _inner;

        public CancellableAsyncNotificationHandlerWrapper(ICancellableAsyncNotificationHandler<TNotification> inner)
        {
            _inner = inner;
        }

        public override Task Handle(INotification message, CancellationToken cancellationToken)
        {
            return _inner.Handle((TNotification)message, cancellationToken);
        }
    }
}