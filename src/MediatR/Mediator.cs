namespace MediatR
{
    using Internal;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly ServiceFactory _serviceFactory;
        private static readonly ConcurrentDictionary<Type, RequestHandlerBase> _requestHandlers = new ConcurrentDictionary<Type, RequestHandlerBase>();
        private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> _notificationHandlers = new ConcurrentDictionary<Type, NotificationHandlerWrapper>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceFactory">The single instance factory.</param>
        public Mediator(ServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();

            var handler = (RequestHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(requestType,
                t => (RequestHandlerBase)Activator.CreateInstance(typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse))));

            return handler.HandleAsync(request, cancellationToken, _serviceFactory);
        }

        public Task<object?> SendAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            var requestType = request.GetType();
            var requestInterfaceType = requestType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
            var isValidRequest = requestInterfaceType != null;

            if (!isValidRequest)
            {
                throw new ArgumentException($"{nameof(request)} does not implement ${nameof(IRequest)}");
            }

            var responseType = requestInterfaceType!.GetGenericArguments()[0];
            var handler = _requestHandlers.GetOrAdd(requestType,
                t => (RequestHandlerBase)Activator.CreateInstance(typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType)));

            // call via dynamic dispatch to avoid calling through reflection for performance reasons
            return handler.HandleAsync(request, cancellationToken, _serviceFactory);
        }

        public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
             where TNotification : INotification
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            return PublishNotificationAsync(notification, cancellationToken);
        }

        public Task PublishAsync(object notification, CancellationToken cancellationToken = default)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }
            if (notification is INotification instance)
            {
                return PublishNotificationAsync(instance, cancellationToken);
            }

            throw new ArgumentException($"{nameof(notification)} does not implement ${nameof(INotification)}");
        }

        /// <summary>
        /// Override in a derived class to control how the tasks are awaited. By default the implementation is a foreach and await of each handler
        /// </summary>
        /// <param name="allHandlers">Enumerable of tasks representing invoking each notification handler</param>
        /// <param name="notification">The notification being published</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing invoking all handlers</returns>
        protected virtual async Task PublishCoreAsync(IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers, INotification notification, CancellationToken cancellationToken)
        {
            foreach (var handler in allHandlers)
            {
                await handler(notification, cancellationToken).ConfigureAwait(false);
            }
        }


        private Task PublishNotificationAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            var notificationType = notification.GetType();
            var handler = _notificationHandlers.GetOrAdd(notificationType,
                t => (NotificationHandlerWrapper)Activator.CreateInstance(typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType)));

            return handler.HandleAsync(notification, cancellationToken, _serviceFactory, PublishCoreAsync);
        }
    }
}
