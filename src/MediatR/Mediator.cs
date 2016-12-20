namespace MediatR
{
    using Internal;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly SingleInstanceFactory _singleInstanceFactory;
        private readonly MultiInstanceFactory _multiInstanceFactory;
        private static readonly ConcurrentDictionary<Type, RequestHandler> _voidRequestHandlers = new ConcurrentDictionary<Type, RequestHandler>();
        private static readonly ConcurrentDictionary<Type, object> _requestHandlers = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, object> _pipelineWrappers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="singleInstanceFactory">The single instance factory.</param>
        /// <param name="multiInstanceFactory">The multi instance factory.</param>
        public Mediator(SingleInstanceFactory singleInstanceFactory, MultiInstanceFactory multiInstanceFactory)
        {
            _singleInstanceFactory = singleInstanceFactory;
            _multiInstanceFactory = multiInstanceFactory;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestType = request.GetType();

            var handler = (RequestHandler<TResponse>)_requestHandlers.GetOrAdd(requestType,
                t => Activator.CreateInstance(typeof(RequestHandlerImpl<,>).MakeGenericType(requestType, typeof(TResponse))));

            return handler.Handle(request, cancellationToken, _singleInstanceFactory, _multiInstanceFactory);
        }

        public Task SendAsync(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestType = request.GetType();

            var handler = _voidRequestHandlers.GetOrAdd(requestType,
                t => (RequestHandler) Activator.CreateInstance(typeof(RequestHandlerImpl<>).MakeGenericType(requestType)));

            return handler.Handle(request, cancellationToken, _singleInstanceFactory, _multiInstanceFactory);
        }

        public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken))
            where TNotification : INotification
        {
            var notificationType = typeof(TNotification);
            var notificationHandlers = _multiInstanceFactory(typeof(INotificationHandler<>).MakeGenericType(notificationType))
                .Cast<INotificationHandler<TNotification>>()
                .Select(handler =>
                {
                    handler.Handle(notification);
                    return Unit.Task;
                });
            var asyncNotificationHandlers = _multiInstanceFactory(typeof(IAsyncNotificationHandler<>).MakeGenericType(notificationType))
                .Cast<IAsyncNotificationHandler<TNotification>>()
                .Select(handler => handler.Handle(notification));
            var cancellableAsyncNotificationHandlers = _multiInstanceFactory(typeof(ICancellableAsyncNotificationHandler<>).MakeGenericType(notificationType))
                .Cast<ICancellableAsyncNotificationHandler<TNotification>>()
                .Select(handler => handler.Handle(notification, cancellationToken));

            var allHandlers = notificationHandlers
                .Concat(asyncNotificationHandlers)
                .Concat(cancellableAsyncNotificationHandlers);

            return Task.WhenAll(allHandlers);
        }
    }
}
