namespace MediatR
{
    using System;
    using System.Collections.Generic;
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

            var mediator = _serviceFactory.GetInstance<INotificationMediator<TNotification>>();
            return mediator.Publish(notification, cancellationToken, PublishBehavior);
        }

        /// <summary>
        /// Override in a derived class to control how the tasks are awaited. By default the implementation is <see cref="Task.WhenAll(IEnumerable{Task})" />
        /// </summary>
        /// <param name="allHandlers">Enumerable of tasks representing invoking each notification handler</param>
        /// <returns>A task representing invoking all handlers</returns>
        protected virtual Task PublishBehavior(IEnumerable<Task> allHandlers) =>
            Task.WhenAll(allHandlers);
    }
}
