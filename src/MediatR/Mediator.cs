namespace MediatR
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly ServiceFactory _serviceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceFactory">The single instance factory.</param>
        public Mediator(ServiceFactory serviceFactory) =>
            _serviceFactory = serviceFactory;

        /// <inheritdoc />
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();
            var mediatorType = typeof(IRequestMediator<,>).MakeGenericType(requestType, typeof(TResponse));
            var mediator = (IRequestMediator<TResponse>) _serviceFactory(mediatorType);

            return mediator.Send(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var mediator = (INotificationMediator<TNotification>)_serviceFactory(typeof(INotificationMediator<TNotification>));
            return mediator.Publish(notification, cancellationToken);
        }
    }
}
