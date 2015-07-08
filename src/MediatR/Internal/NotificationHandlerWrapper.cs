namespace MediatR.Internal
{
    internal abstract class NotificationHandlerWrapper
    {
        public abstract void Handle(INotification message);
    }

    internal class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> _inner;

        public NotificationHandlerWrapper(INotificationHandler<TNotification> inner)
        {
            _inner = inner;
        }

        public override void Handle(INotification message)
        {
            _inner.Handle((TNotification)message);
        }
    }
}