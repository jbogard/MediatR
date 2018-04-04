using MediatR.Pipeline;

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
        private readonly SingleInstanceFactory _singleInstanceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="singleInstanceFactory">The single instance factory.</param>
        public Mediator(SingleInstanceFactory singleInstanceFactory)
        {
            _singleInstanceFactory = singleInstanceFactory;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var handlerType = typeof(RequestProcessor<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = (UntypedRequestProcessor<TResponse>)_singleInstanceFactory(handlerType);

            return handler.Handle(request, cancellationToken);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var handlerType = typeof(RequestProcessor<>).MakeGenericType(request.GetType());
            var handler = (UntypedRequestProcessor<Unit>)_singleInstanceFactory(handlerType);

            return handler.Handle(request, cancellationToken);
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken))
            where TNotification : INotification
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var handler = (NotificationProcessor<TNotification>)_singleInstanceFactory(typeof(NotificationProcessor<TNotification>));
            return handler.Handle(notification, cancellationToken, PublishCore);
        }

        /// <summary>
        /// Override in a derived class to control how the tasks are awaited. By default the implementation is <see cref="Task.WhenAll(IEnumerable{Task})" />
        /// </summary>
        /// <param name="allHandlers">Enumerable of tasks representing invoking each notification handler</param>
        /// <returns>A task representing invoking all handlers</returns>
        protected virtual Task PublishCore(IEnumerable<Task> allHandlers)
        {
            return Task.WhenAll(allHandlers);
        }
    }
}
